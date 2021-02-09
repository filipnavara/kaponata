// <copyright file="AdbClient.Shell.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> shell methods for the <see cref="AdbClient"/> class.
    /// </summary>
    public partial class AdbClient
    {
        /// <summary>
        /// Connects to the sync service.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="shellCommand">
        /// The shell command to execute.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The output <see cref="ShellStream"/>.
        /// </returns>
        public virtual async Task<ShellStream> ExecuteRemoteShellCommandAsync(DeviceData device, string shellCommand, CancellationToken cancellationToken)
        {
            var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"shell:{shellCommand}", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            return protocol.GetShellStream();
        }

        /// <summary>
        /// Creates a directory for a given path.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        /// <param name="path">
        /// The path to be created.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        internal async Task CreateDirectoryAsync(DeviceData device, string path, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            string command = $"mkdir -p {path}";
            using var stream = await this.ExecuteRemoteShellCommandAsync(device, command, cancellationToken).ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            var output = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a directory for a given path.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        /// <param name="path">
        /// The path to be created.
        /// </param>
        /// <param name="permissions">
        /// The permissions to give to the path.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        internal async Task ChModAsync(DeviceData device, string path, string permissions, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (await this.ExistsAsync(device, path, cancellationToken).ConfigureAwait(false))
            {
                string command = string.Format(@"chmod {1} {0}", path, permissions);
                using var stream = await this.ExecuteRemoteShellCommandAsync(device, command, cancellationToken).ConfigureAwait(false);
                using var streamReader = new StreamReader(stream);
                var output = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
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
        internal async Task<bool> ExistsAsync(DeviceData device, string path, CancellationToken cancellationToken)
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
        internal async Task<bool> IsDirectoryAsync(DeviceData device, string path, CancellationToken cancellationToken)
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
        /// Deletes a file if the path exists on the device.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        /// <param name="path">
        /// The path to be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        internal async Task DeleteFileIfExistsAsync(DeviceData device, string path, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (await this.ExistsAsync(device, path, cancellationToken).ConfigureAwait(false))
            {
                var command = string.Format(@"rm -f {0}", path);
                if (await this.IsDirectoryAsync(device, path, cancellationToken).ConfigureAwait(false))
                {
                    command = string.Format(@"rm -r -f {0}", path);
                }

                using var stream = await this.ExecuteRemoteShellCommandAsync(device, command, cancellationToken).ConfigureAwait(false);
                using var streamReader = new StreamReader(stream);
                var output = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
