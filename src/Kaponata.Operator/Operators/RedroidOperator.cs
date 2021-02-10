// <copyright file="RedroidOperator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// The <see cref="RedroidOperator"/> detects Android running in Docker containers and creates <see cref="MobileDevice"/>
    /// objects.
    /// </summary>
    public class RedroidOperator : BackgroundService
    {
        private readonly ILogger<RedroidOperator> logger;
        private readonly KubernetesClient kubernetes;

        /// <summary>
        /// A semaphore which prevents multiple iterations of the <see cref="ReconcileAsync(CancellationToken)"/>
        /// task running in parallel.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private TaskCompletionSource waitForCompletionTcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedroidOperator"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A <see cref="KubernetesClient"/> object which provides access to the Kubernetes API server.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public RedroidOperator(KubernetesClient kubernetes, ILogger<RedroidOperator> logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a value indicating whether the operator is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a <see cref="Task"/> which will finish when the <see cref="RedroidOperator"/> stops executing.
        /// </summary>
        public Task WaitForCompletion => this.waitForCompletionTcs.Task;

        /// <summary>
        /// Gets the labels attached to devices managed by the <see cref="RedroidOperator"/> class.
        /// </summary>
        private Dictionary<string, string> DeviceLabels
            => new ()
            {
                // kubernetes.io/os=android
                { Annotations.Os, Annotations.OperatingSystem.Android },

                // app.kubernetes.io/managedBy=RedroidOperator
                { Annotations.ManagedBy, nameof(RedroidOperator) },
            };

        /// <summary>
        /// Gets a selector which can be used to list devices managed by the <see cref="RedroidOperator"/> class.
        /// </summary>
        private string DeviceLabelSelector
            => Selector.Create(this.DeviceLabels);

        /// <summary>
        /// Gets a selector which can be used to list Redroid pods.
        /// </summary>
        private string PodLabelSelector
            => Selector.Create(
                new ()
                {
                    // Assume that all pods running the Android operation system are emulators
                    // kubernetes.io/os=android
                    { Annotations.Os, Annotations.OperatingSystem.Android },
                });

        /// <summary>
        /// The main reconciliation loop for the <see cref="RedroidOperator"/>. Lists all Redroid pods and Redroid mobile device objects
        /// (by using selector labels). For each Redroid pod, the creates a mobile device object if needed. Finally, the loop
        /// removes all mobile device objects which refer to Redroid pods which no longer exist.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task<bool> ReconcileAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting the reconciliation loop");

            this.logger.LogInformation("Acquiring the sempahore.");
            await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation("Acquired the semaphore.");

            try
            {
                // Enumerate all Android emulator pods
                this.logger.LogInformation("Listing all emulator pods");
                var pods = await this.kubernetes.ListPodAsync(labelSelector: this.PodLabelSelector, cancellationToken: cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Found {count} emulator pods", pods.Items.Count);

                // Enumerate all Android emulator devices
                this.logger.LogInformation("Listing all emulator devices");
                var devices = await this.kubernetes.ListMobileDeviceAsync(labelSelector: this.DeviceLabelSelector, cancellationToken: cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Found {count} emulator devices", devices.Items.Count);

                // Loop over all Android emulator pods. Process them one by one; remove the equivalent device from the device list
                // when processing is done. Any device which persists is an obsolete device.
                foreach (var pod in pods.Items)
                {
                    if (pod.Status.Phase != "Running")
                    {
                        this.logger.LogInformation("The pod '{pod}' is in phase '{phase}'. Skipping", pod.Metadata.Name, pod.Status.Phase);
                        continue;
                    }

                    if (!pod.Status.ContainerStatuses.All(c => c.Ready))
                    {
                        this.logger.LogInformation("The pod '{pod}' is not ready. Skipping", pod.Metadata.Name);
                        continue;
                    }

                    var device = devices.Items.SingleOrDefault(d => d.Metadata.Name == pod.Metadata.Name);

                    if (device == null)
                    {
                        device =
                            await this.kubernetes.CreateMobileDeviceAsync(
                                new MobileDevice()
                                {
                                    Metadata = new V1ObjectMeta()
                                    {
                                        Name = pod.Metadata.Name,
                                        NamespaceProperty = pod.Metadata.NamespaceProperty,
                                        Labels = this.DeviceLabels,
                                    },
                                    Spec = new MobileDeviceSpec()
                                    {
                                        Owner = pod.Metadata.Name,
                                    },
                                },
                                cancellationToken).ConfigureAwait(false);

                        this.logger.LogInformation("Created device '{device}' for emulator pod '{pod}'", device.Metadata.Name, pod.Metadata.Name);
                    }
                    else
                    {
                        devices.Items.Remove(device);
                        this.logger.LogInformation("Device '{device}' exists for emulator pod '{pod}'", device.Metadata.Name, pod.Metadata.Name);
                    }
                }

                // Remove all obsolete devices.
                foreach (var device in devices.Items)
                {
                    this.logger.LogInformation("Deleting obsolete device {device}", device.Metadata.Name);
                    await this.kubernetes.DeleteMobileDeviceAsync(device, TimeSpan.FromMinutes(1), cancellationToken: cancellationToken).ConfigureAwait(false);
                    this.logger.LogInformation("Deleted obsolete device {device}", device.Metadata.Name);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while executing the {operatorName} reconciliation loop: {message}", this.GetType().Name, ex.Message);
                return false;
            }
            finally
            {
                this.logger.LogInformation("Releasing the semaphore");
                this.semaphore.Release();
                this.logger.LogInformation("Released the semaphore");
            }

            return true;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.waitForCompletionTcs = new TaskCompletionSource();
            this.IsRunning = true;

            this.logger.LogInformation("Starting the {operatorName} operator", this.GetType().Name);

            try
            {
                // Start a first reconciliation loop.
                await this.ReconcileAsync(cancellationToken).ConfigureAwait(false);

                var watchTasks = new Task<WatchExitReason>[]
                {
                    this.kubernetes.WatchMobileDeviceAsync(
                        fieldSelector: null,
                        labelSelector: this.DeviceLabelSelector,
                        resourceVersion: null,
                        async (eventType, device) =>
                        {
                            await this.ReconcileAsync(cancellationToken);
                            return WatchResult.Continue;
                        },
                        cancellationToken),
                    this.kubernetes.WatchPodAsync(
                        fieldSelector: null,
                        labelSelector: this.PodLabelSelector,
                        resourceVersion: null,
                        async (eventType, pod) =>
                        {
                            await this.ReconcileAsync(cancellationToken);
                            return WatchResult.Continue;
                        },
                        cancellationToken),
                };

                var exitTask = await Task.WhenAny(watchTasks);
                var exitReason = await exitTask.ConfigureAwait(false);

                this.logger.LogInformation("Exiting operator {operatorName} because a watch task exited with reason {exitReason}", this.GetType().Name, exitReason);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An unexpected exception occurred when running the {operatorName} operator: {message}.", this.GetType().Name, ex.Message);
            }
            finally
            {
                this.waitForCompletionTcs.SetResult();
                this.IsRunning = false;
            }
        }
    }
}
