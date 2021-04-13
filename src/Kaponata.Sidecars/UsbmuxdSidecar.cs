// <copyright file="UsbmuxdSidecar.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Muxer;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Sidecars
{
    /// <summary>
    /// The <see cref="UsbmuxdSidecar"/> publishes all devices reported by the iOS USB multiplexor into the Kubernetes cluster.
    /// </summary>
    public class UsbmuxdSidecar : BackgroundService
    {
        private readonly MuxerClient muxerClient;
        private readonly KubernetesClient kubernetesClient;
        private readonly NamespacedKubernetesClient<MobileDevice> deviceClient;
        private readonly UsbmuxdSidecarConfiguration configuration;
        private readonly ILogger<UsbmuxdSidecar> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbmuxdSidecar"/> class.
        /// </summary>
        /// <param name="muxerClient">
        /// A <see cref="MuxerClient"/> which represents a connection to the local iOS USB muxer.
        /// </param>
        /// <param name="kubernetes">
        /// A <see cref="KubernetesClient"/> which represents a connection to the Kubernetes cluster.
        /// </param>
        /// <param name="configuration">
        /// A <see cref="UsbmuxdSidecarConfiguration"/> which represents the configuration for this sidecar.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used to log messages.
        /// </param>
        public UsbmuxdSidecar(MuxerClient muxerClient, KubernetesClient kubernetes, UsbmuxdSidecarConfiguration configuration, ILogger<UsbmuxdSidecar> logger)
        {
            this.muxerClient = muxerClient ?? throw new ArgumentNullException(nameof(muxerClient));
            this.kubernetesClient = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.deviceClient = this.kubernetesClient.GetClient<MobileDevice>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbmuxdSidecar"/> class. Intended for mocking purposes only.
        /// </summary>
        protected UsbmuxdSidecar()
        {
        }

        /// <summary>
        /// Asynchronously reconciles the devices returned by the iOS device multiplexor with the devices available in the cluster.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task ReconcileAsync(CancellationToken cancellationToken)
        {
            // We list:
            // - Only devices which are managed by _this_ usbmuxd instance. All those devices should have this pod
            //   set at their owner, but there isn't a straightforward way to filter on owner.
            //   Hence, we use a label selector for that.
            // - Only usbmuxd devices
            try
            {
                this.logger.LogInformation("Running an reconcilation loop for pod {podName}", this.configuration.PodName);

                var parent = await this.kubernetesClient.GetClient<V1Pod>().TryReadAsync(this.configuration.PodName, cancellationToken).ConfigureAwait(false);

                if (parent == null)
                {
                    throw new InvalidOperationException($"Could not find the parent pod {this.configuration.PodName}");
                }

                this.logger.LogInformation("Listing all devices in the Kubernetes cluster.");
                var kubernetesDevices = await this.deviceClient.ListAsync(
                    labelSelector: LabelSelector.Create(
                        new Dictionary<string, string>()
                        {
                            { Annotations.Os, Annotations.OperatingSystem.iOS },
                            { Annotations.ManagedBy, nameof(UsbmuxdSidecar) },
                            { Annotations.Instance, this.configuration.PodName },
                        }),
                    cancellationToken: cancellationToken);
                this.logger.LogInformation("Found {count} devices", kubernetesDevices.Items.Count);

                this.logger.LogInformation("Listing all usbmuxd devices");
                var muxerDevices = await this.muxerClient.ListDevicesAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Found {count} usbmuxd devices", muxerDevices.Count);

                foreach (var muxerDevice in muxerDevices)
                {
                    this.logger.LogInformation("Processing device {device}", muxerDevice.Udid);
                    var kubernetesDevice = kubernetesDevices.Items.SingleOrDefault(d => string.Equals(d.Metadata.Name, muxerDevice.Udid, StringComparison.OrdinalIgnoreCase));

                    // Pairing records can be stored at both the cluster level and locally. Use the first pairing record which is valid, and make sure the cluster
                    // and local records are in sync.
                    var usbmuxdPairingRecord = await this.muxerClient.ReadPairingRecordAsync(muxerDevice.Udid, cancellationToken).ConfigureAwait(false);

                    if (kubernetesDevice == null)
                    {
                        this.logger.LogInformation("Creating a new device {device}", muxerDevice.Udid);

                        // Register the device
                        kubernetesDevice = new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                Labels = new Dictionary<string, string>()
                                {
                                    { Annotations.Os, Annotations.OperatingSystem.iOS },
                                    { Annotations.ManagedBy, nameof(UsbmuxdSidecar) },
                                    { Annotations.Instance, this.configuration.PodName },
                                },
                                Name = muxerDevice.Udid,
                                NamespaceProperty = parent.Metadata.NamespaceProperty,
                                OwnerReferences = new V1OwnerReference[]
                                {
                                    parent.AsOwnerReference(blockOwnerDeletion: false, controller: false),
                                },
                            },
                            Spec = new MobileDeviceSpec()
                            {
                                Owner = this.configuration.PodName,
                            },
                        };

                        await this.deviceClient.CreateAsync(kubernetesDevice, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogInformation("The device {device} already exists; no action needed.", muxerDevice.Udid);

                        // Nothing to do; though we could verify the properties.
                        kubernetesDevices.Items.Remove(kubernetesDevice);
                    }
                }

                // All live usbmuxd devices have been removed from muxerDevice.Items by the loop above. The remaining
                // devices are stale; remove them.
                foreach (var kubernetesDevice in kubernetesDevices.Items)
                {
                    this.logger.LogInformation("Removing stale device {device}.", kubernetesDevice.Metadata.Name);
                    await this.deviceClient.DeleteAsync(kubernetesDevice, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while running the reconciliation loop. {message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.ReconcileAsync(stoppingToken);

            await this.muxerClient.ListenAsync(
                async (attached, ct) =>
                {
                    this.logger.LogInformation("Got an attached message for device {deviceId} ({udid}). Starting reconcilation.", attached.DeviceID, attached.Properties.SerialNumber);
                    await this.ReconcileAsync(stoppingToken);
                    return MuxerListenAction.ContinueListening;
                },
                async (detached, ct) =>
                {
                    this.logger.LogInformation("Got a detached message for device {deviceId}. Starting reconcilation.", detached.DeviceID);
                    await this.ReconcileAsync(stoppingToken);
                    return MuxerListenAction.ContinueListening;
                },
                async (trusted, ct) =>
                {
                    this.logger.LogInformation("Got a trusted message for device {deviceId} ({udid}). Starting reconcilation.", trusted.DeviceID);
                    await this.ReconcileAsync(stoppingToken);
                    return MuxerListenAction.ContinueListening;
                },
                stoppingToken).ConfigureAwait(false);
        }
    }
}
