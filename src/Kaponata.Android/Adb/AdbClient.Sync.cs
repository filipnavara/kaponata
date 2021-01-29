// <copyright file="AdbClient.Sync.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> sync service methods for the <see cref="AdbClient"/> class.
    /// </summary>
    public partial class AdbClient
    {
        /// <summary>
        /// Lists the files from the remote directory.
        /// </summary>
        /// <param name="device">
        /// The device on which the <c>apk</c> needs to be installed.
        /// </param>
        /// <param name="remotePath">
        /// The remote path.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task<IList<FileStatistics>> GetDirectoryListingAsync(DeviceData device, string remotePath, CancellationToken cancellationToken)
        {
            await using var protocol = await this.ConnectToSyncServiceAsync(device, cancellationToken).ConfigureAwait(false);

            List<FileStatistics> entries = new List<FileStatistics>();

            await protocol.WriteSyncCommandAsync(SyncCommandType.LIST, remotePath, cancellationToken).ConfigureAwait(false);

            while (true)
            {
                var response = await protocol.ReadSyncCommandTypeAsync(cancellationToken).ConfigureAwait(false);

                if (response == SyncCommandType.DENT)
                {
                    var entry = await protocol.ReadFileStatisticsAsync(cancellationToken).ConfigureAwait(false);
                    entries.Add(entry);
                }
                else if (response == SyncCommandType.DONE)
                {
                    break;
                }
                else
                {
                    throw new AdbException($"The server returned an invalid sync response: {response}");
                }
            }

            return entries;
        }

        /// <summary>
        /// Connects to the sync service.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A connection to the sync service.
        /// </returns>
        public virtual async Task<AdbProtocol> ConnectToSyncServiceAsync(DeviceData device, CancellationToken cancellationToken)
        {
            var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);
            await protocol.StartSyncSessionAsync(cancellationToken).ConfigureAwait(false);
            return protocol;
        }
    }
}
