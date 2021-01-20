// <copyright file="AdbClient.Commands.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> commands for the <see cref="AdbClient"/> class.
    /// </summary>
    public partial class AdbClient
    {
        /// <summary>
        /// Gets all connected devices listed by the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The list of connected devices.
        /// </returns>
        public async Task<IList<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken)
        {
            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            // send devices command.
            await protocol.WriteAsync("host:devices-l", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            // read devices response.
            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var devicesMessage = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            // Parse devices.
            var devices = devicesMessage.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return devices.Select(d => DeviceData.Parse(d)).ToList();
        }

        /// <summary>
        /// Gets all connected devices listed by the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The list of connected devices.
        /// </returns>
        public async Task<int> GetAdbVersionAsync(CancellationToken cancellationToken)
        {
            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            // send version command.
            await protocol.WriteAsync("host:version", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            // read version response.
            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var versionMessage = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            return int.Parse(versionMessage, NumberStyles.HexNumber);
        }
    }
}
