// <copyright file="AdbClient.Listen.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> listen methods for the <see cref="AdbClient"/> class.
    /// </summary>
    public partial class AdbClient
    {
        /// <summary>
        /// Listens for device notifications on a <see cref="AdbProtocol"/>.
        /// </summary>
        /// <param name="onNewDeviceList">
        /// The action to take when a new device list is received.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation. Returns <see langword="true"/> when
        /// the client aborted the listen operation, <see langword="false"/> when the server disconnected.
        /// </returns>
        public async Task<bool> ListenAsync(
            Func<List<DeviceData>, Task<AdbListenAction>> onNewDeviceList,
            CancellationToken cancellationToken)
        {
            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            if (protocol == null)
            {
                throw new InvalidOperationException("Could not connect to the ADB server.");
            }

            // setup track devices service
            await protocol.WriteAsync("host:track-devices", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            string deviceData;
            while ((deviceData = await this.ReadDeviceEventAsync(protocol, cancellationToken).ConfigureAwait(false)) != null)
            {
                string[] deviceValues = deviceData.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                var devices = deviceValues.Select(d => DeviceData.Parse(d));

                if (onNewDeviceList != null)
                {
                    if (await onNewDeviceList(devices.ToList()).ConfigureAwait(false) == AdbListenAction.StopListening)
                    {
                        break;
                    }
                }
            }

            // If message is null, the server closed the connection. Let the caller know.
            if (deviceData == null)
            {
                this.logger.LogWarning("The server unexpectedly close the connection when tracking for devices.");
            }

            return deviceData != null;
        }

        /// <summary>
        /// Reads the next device event.
        /// </summary>
        /// <param name="protocol">
        /// The <see cref="AdbProtocol"/> instance to be used.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The device event data.
        /// </returns>
        public async Task<string> ReadDeviceEventAsync(AdbProtocol protocol, CancellationToken cancellationToken)
        {
            var length = await protocol.ReadUInt16HexAsync(cancellationToken).ConfigureAwait(false);
            return await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);
        }
    }
}
