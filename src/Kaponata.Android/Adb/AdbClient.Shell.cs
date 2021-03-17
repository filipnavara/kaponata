// <copyright file="AdbClient.Shell.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

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
    }
}
