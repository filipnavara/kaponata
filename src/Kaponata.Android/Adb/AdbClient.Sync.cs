// <copyright file="AdbClient.Sync.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> sync service methods for the <see cref="AdbClient"/> class.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/core.git/+/brillo-m7-dev/adb/SYNC.TXT"/>
    public partial class AdbClient
    {
        private const int MaxPathLength = 1024;

        /// <summary>
        /// Lists the files from the remote directory.
        /// </summary>
        /// <param name="device">
        /// The device to be connected to the sync service.
        /// </param>
        /// <param name="remotePath">
        /// The remote path.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A list of <see cref="FileStatistics"/>.
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
        /// Pulls a remote file and writes it to the output stream.
        /// </summary>
        /// <param name="device">
        /// The device to be connected to the sync service.
        /// </param>
        /// <param name="remotePath">
        /// The remote path from which the file needs to be pulled.
        /// </param>
        /// <param name="stream">
        /// The output stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task PullAsync(DeviceData device, string remotePath, Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (string.IsNullOrEmpty(remotePath))
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            await using var protocol = await this.ConnectToSyncServiceAsync(device, cancellationToken).ConfigureAwait(false);
            await protocol.WriteSyncCommandAsync(SyncCommandType.RECV, remotePath, cancellationToken).ConfigureAwait(false);
            await protocol.ReadSyncDataAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Pushes a file to the remote path.
        /// </summary>
        /// <param name="device">
        /// The device to be connected to the sync service.
        /// </param>
        /// <param name="stream">
        /// The input stream.
        /// </param>
        /// <param name="remotePath">
        /// The remote path on which the file needs to be pushed.
        /// </param>
        /// <param name="permissions">
        /// The permissions of the remote file.
        /// </param>
        /// <param name="timestamp">
        /// The timestamp of the remote file.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task PushAsync(DeviceData device, Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (string.IsNullOrEmpty(remotePath))
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            await using var protocol = await this.ConnectToSyncServiceAsync(device, cancellationToken).ConfigureAwait(false);
            await protocol.WriteSyncCommandAsync(SyncCommandType.SEND, $"{remotePath},{permissions}", cancellationToken).ConfigureAwait(false);
            await protocol.WriteSyncDataAsync(stream, cancellationToken).ConfigureAwait(false);

            int time = (int)timestamp.ToUnixTimeSeconds();
            await protocol.WriteSyncCommandAsync(SyncCommandType.DONE, time, cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Indicates if the path exists on the given device.
        /// </summary>
        /// <param name="device">
        /// The device for which check if the path exists.
        /// </param>
        /// <param name="path">
        /// The path to be verified.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A value indicating whether the path exists on the given device.
        /// </returns>
        public async Task<bool> ExistsAsync(DeviceData device, string path, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var fileStat = await this.GetFileStatisticsAsync(device, path, cancellationToken).ConfigureAwait(false);

            return fileStat.FileMode != 0;
        }

        /// <summary>
        /// Indicates if the path is a directory on the given device.
        /// </summary>
        /// <param name="device">
        /// The device for which check the path.
        /// </param>
        /// <param name="path">
        /// The path to be verified.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A value indicting whether the path is an directory on the given device.
        /// </returns>
        public async Task<bool> IsDirectoryAsync(DeviceData device, string path, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var fileStat = await this.GetFileStatisticsAsync(device, path, cancellationToken).ConfigureAwait(false);

            return (fileStat.FileMode & 0x4000) == 0x4000;
        }

        /// <summary>
        /// Gets the <see cref="FileStatistics"/> of a remote file.
        /// </summary>
        /// <param name="device">
        /// The device to be connected to the sync service.
        /// </param>
        /// <param name="remotePath">
        /// The remote path on which the file needs to be pushed.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="FileStatistics"/> for the remote file.
        /// </returns>
        public async Task<FileStatistics> GetFileStatisticsAsync(DeviceData device, string remotePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(remotePath))
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            await using var protocol = await this.ConnectToSyncServiceAsync(device, cancellationToken).ConfigureAwait(false);
            await protocol.WriteSyncCommandAsync(SyncCommandType.STAT, remotePath, cancellationToken).ConfigureAwait(false);
            var response = await protocol.ReadSyncCommandTypeAsync(cancellationToken).ConfigureAwait(false);
            if (response != SyncCommandType.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response: {response}");
            }

            var fileStatistics = await protocol.ReadFileStatisticsAsync(cancellationToken).ConfigureAwait(false);
            fileStatistics.Path = remotePath;

            return fileStatistics;
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
