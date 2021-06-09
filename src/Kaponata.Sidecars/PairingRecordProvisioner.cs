// <copyright file="PairingRecordProvisioner.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.Workers;
using Kaponata.Kubernetes.PairingRecords;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Sidecars
{
    /// <summary>
    /// Selects a pairing record which can be used with a device, by selecting a pairing record which is available in the muxer,
    /// in the cluster, or by creating a new pairing record.
    /// </summary>
    public class PairingRecordProvisioner
    {
        private readonly MuxerClient muxerClient;
        private readonly ILogger<PairingRecordProvisioner> logger;
        private readonly KubernetesPairingRecordStore kubernetesPairingRecordStore;
        private readonly DeviceServiceProvider serviceProvider;
        private readonly Dictionary<string, Task> pairingTasks = new Dictionary<string, Task>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingRecordProvisioner"/> class.
        /// </summary>
        /// <param name="muxerClient">
        /// A <see cref="MuxerClient"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="kubernetesPairingRecordStore">
        /// A <see cref="PairingRecordStore"/> which provides access to the pairing records stored in the cluster.
        /// </param>
        /// <param name="serviceProvider">
        /// A <see cref="ServiceProvider"/> which can be used to acquire services.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public PairingRecordProvisioner(MuxerClient muxerClient, KubernetesPairingRecordStore kubernetesPairingRecordStore, DeviceServiceProvider serviceProvider, ILogger<PairingRecordProvisioner> logger)
        {
            this.muxerClient = muxerClient ?? throw new ArgumentNullException(nameof(muxerClient));
            this.kubernetesPairingRecordStore = kubernetesPairingRecordStore ?? throw new ArgumentNullException(nameof(kubernetesPairingRecordStore));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingRecordProvisioner"/> class. Intended for mocking purposes only.
        /// </summary>
        protected PairingRecordProvisioner()
        {
        }

        /// <summary>
        /// Asynchronously retrieves or generates a pairing record for a device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device for which to retrieve or generate the pairing record.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task<PairingRecord> ProvisionPairingRecordAsync(string udid, CancellationToken cancellationToken)
        {
            // Pairing records can be stored at both the cluster level and locally. Use the first pairing record which is valid, and make sure the cluster
            // and local records are in sync.
            this.logger.LogInformation("Provisioning a pairing record for device {udid}", udid);

            var usbmuxdPairingRecord = await this.muxerClient.ReadPairingRecordAsync(udid, cancellationToken).ConfigureAwait(false);
            var kubernetesPairingRecord = await this.kubernetesPairingRecordStore.ReadAsync(udid, cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Found pairing record {usbmuxdPairingRecord} in usbmuxd", usbmuxdPairingRecord);
            this.logger.LogInformation("Found pairing record {kubernetesPairingRecord} in Kubernetes", kubernetesPairingRecord);

            PairingRecord pairingRecord = null;

            using (var context = await this.serviceProvider.CreateDeviceScopeAsync(udid, cancellationToken).ConfigureAwait(false))
            await using (var lockdown = await context.StartServiceAsync<LockdownClient>(cancellationToken).ConfigureAwait(false))
            {
                if (usbmuxdPairingRecord != null && await lockdown.ValidatePairAsync(usbmuxdPairingRecord, cancellationToken).ConfigureAwait(false))
                {
                    this.logger.LogInformation("The pairing record stored in usbmuxd is valid.");
                    pairingRecord = usbmuxdPairingRecord;
                }
                else if (kubernetesPairingRecord != null && await lockdown.ValidatePairAsync(kubernetesPairingRecord, cancellationToken).ConfigureAwait(false))
                {
                    this.logger.LogInformation("The pairing record stored in Kubernetes is valid.");
                    pairingRecord = kubernetesPairingRecord;
                }
                else
                {
                    this.logger.LogInformation("No valid pairing record could be found.");

                    if (!this.pairingTasks.ContainsKey(udid) || this.pairingTasks[udid].IsCompleted)
                    {
                        this.logger.LogInformation("Starting a new pairing task");
                        var worker = context.ServiceProvider.GetRequiredService<PairingWorker>();
                        this.pairingTasks[udid] = worker.PairAsync(cancellationToken);
                    }

                    this.logger.LogInformation("A pairing task is running. Returning null and waiting for the device to pair with the host.");
                    return null;
                }
            }

            // Update outdated pairing records if required.
            if (!PairingRecord.Equals(pairingRecord, usbmuxdPairingRecord))
            {
                this.logger.LogInformation("The pairing record stored in usbmuxd for device {device} is outdated. Updating.", udid);
                if (usbmuxdPairingRecord != null)
                {
                    await this.muxerClient.DeletePairingRecordAsync(udid, cancellationToken).ConfigureAwait(false);
                }

                await this.muxerClient.SavePairingRecordAsync(udid, pairingRecord, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Updated the pairing record in usbmuxd for device {device}.", udid);
            }

            if (!PairingRecord.Equals(pairingRecord, kubernetesPairingRecord))
            {
                this.logger.LogInformation("The pairing record stored in the cluster for device {device} is outdated. Updating.", udid);
                if (kubernetesPairingRecord != null)
                {
                    await this.kubernetesPairingRecordStore.DeleteAsync(udid, cancellationToken).ConfigureAwait(false);
                }

                await this.kubernetesPairingRecordStore.WriteAsync(udid, pairingRecord, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Updated the pairing record in the cluster for device {device}.", udid);
            }

            return pairingRecord;
        }
    }
}
