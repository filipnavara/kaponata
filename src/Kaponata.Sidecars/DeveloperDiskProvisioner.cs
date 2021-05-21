// <copyright file="DeveloperDiskProvisioner.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.MobileImageMounter;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Sidecars
{
    /// <summary>
    /// Provisions iOS devices with developer disks.
    /// </summary>
    public class DeveloperDiskProvisioner
    {
        private readonly DeviceServiceProvider serviceProvider;
        private readonly DeveloperDiskStore developerDiskStore;
        private readonly ILogger<DeveloperDiskProvisioner> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperDiskProvisioner"/> class.
        /// </summary>
        /// <param name="developerDiskStore">
        /// A <see cref="developerDiskStore"/> which provides access to a registry of developer disk images.
        /// </param>
        /// <param name="serviceProvider">
        /// A <see cref="DeviceServiceProvider"/> from which services, required to connect to iOS devices, can be sourced.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        public DeveloperDiskProvisioner(DeveloperDiskStore developerDiskStore, DeviceServiceProvider serviceProvider, ILogger<DeveloperDiskProvisioner> logger)
        {
            this.developerDiskStore = developerDiskStore ?? throw new ArgumentNullException(nameof(developerDiskStore));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously mounts the developer disk on the device.
        /// </summary>
        /// <param name="udid">
        /// The UDID of the device on which to mount the developer disk.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task returns a value indicating whether the developer disk
        /// is mounted on the device or not.
        /// </returns>
        public virtual async Task<bool> ProvisionDeveloperDiskAsync(string udid, CancellationToken cancellationToken)
        {
            using (var scope = await this.serviceProvider.CreateDeviceScopeAsync(udid, cancellationToken).ConfigureAwait(false))
            await using (var lockdown = await scope.StartServiceAsync<LockdownClient>(cancellationToken).ConfigureAwait(false))
            await using (var imageMounterClient = await scope.StartServiceAsync<MobileImageMounterClient>(cancellationToken).ConfigureAwait(false))
            {
                var developerDiskStatus = await imageMounterClient.LookupImageAsync("Developer", cancellationToken).ConfigureAwait(false);

                // The disk is already mounted.
                if (developerDiskStatus.ImageSignature.Count > 0)
                {
                    var signature = developerDiskStatus.ImageSignature.Count > 0 ? Convert.ToBase64String(developerDiskStatus.ImageSignature[0]) : string.Empty;

                    this.logger.LogWarning("A developer disk has already been mounted on device {udid}: {signature}", udid, signature);
                    return true;
                }

                // Fetch the disk from the store
                var versionString = await lockdown.GetValueAsync("ProductVersion", cancellationToken).ConfigureAwait(false);
                var version = Version.Parse(versionString);

                var developerDisk = await this.developerDiskStore.GetAsync(version, cancellationToken).ConfigureAwait(false);

                if (developerDisk == null && version.Build >= 0)
                {
                    var reducedVersion = new Version(version.Major, version.Minor);
                    this.logger.LogWarning("Could not found the developer disk for version {version}. Attempting to find the developer disk for version {reducedVersion}", version, reducedVersion);
                    developerDisk = await this.developerDiskStore.GetAsync(reducedVersion, cancellationToken).ConfigureAwait(false);
                }

                if (developerDisk == null)
                {
                    this.logger.LogWarning("Could not mount the developer disk on device {udid} because no developer disk is available", udid);
                    return false;
                }

                await imageMounterClient.UploadImageAsync(developerDisk.Image, "Developer", developerDisk.Signature, cancellationToken).ConfigureAwait(false);
                await imageMounterClient.MountImageAsync(developerDisk.Signature, "Developer", cancellationToken).ConfigureAwait(false);

                this.logger.LogInformation("Mounted the developer disk on device {udid}", udid);

                return true;
            }
        }
    }
}
