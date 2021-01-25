// <copyright file="AdbClient.Commands.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

            if (protocol == null)
            {
                throw new InvalidOperationException("Could not connect to the ADB server.");
            }

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

            if (protocol == null)
            {
                throw new InvalidOperationException("Could not connect to the ADB server.");
            }

            // send version command.
            await protocol.WriteAsync("host:version", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            // read version response.
            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var versionMessage = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            var adbVersion = 0;
            var success = int.TryParse(versionMessage, NumberStyles.HexNumber, default, out adbVersion);

            if (!success)
            {
                throw new InvalidOperationException($"Could not parse {versionMessage} to a valid ADB server number.");
            }

            return adbVersion;
        }

        /// <summary>
        /// Installs an <c>apk</c> on the given device.
        /// </summary>
        /// <param name="device">
        /// The device on which the <c>apk</c> needs to be installed.
        /// </param>
        /// <param name="apk">
        /// The <c>apk</c> stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <param name="arguments">
        /// The arguments used to install the <c>apk</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task InstallAsync(DeviceData device, Stream apk, CancellationToken cancellationToken, params string[] arguments)
        {
            this.EnsureDevice(device);

            if (apk == null)
            {
                throw new ArgumentNullException(nameof(apk));
            }

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            var requestBuilder = new StringBuilder();
            requestBuilder.Append("exec:cmd package 'install' ");

            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    requestBuilder.Append(" ");
                    requestBuilder.Append(argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            requestBuilder.Append($" -S {apk.Length}");

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            await protocol.WriteAsync(apk, cancellationToken).ConfigureAwait(false);

            var installMessage = await protocol.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="device"/>
        /// parameter is <see langword="null"/>, and a <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="device"/> does not have a valid serial number.
        /// </summary>
        /// <param name="device">
        /// A <see cref="DeviceData"/> object to validate.
        /// </param>
        internal void EnsureDevice(DeviceData device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(device.Serial))
            {
                throw new ArgumentOutOfRangeException(nameof(device), "You must specific a serial number for the device");
            }
        }
    }
}
