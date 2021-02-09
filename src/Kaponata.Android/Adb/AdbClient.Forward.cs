// <copyright file="AdbClient.Forward.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> forward methods.
    /// </summary>
    /// <remarks>
    /// The forward commands require the <c>host-prefix</c> while the reverse commands connect first to the device and do not send the commands with <c>host-prefix</c>.
    /// </remarks>
    /// <seealso href="https://android.googlesource.com/platform/system/adb/+/refs/heads/master/adb.cpp"/>
    /// <seealso href="https://android.googlesource.com/platform/system/adb/+/refs/heads/master/SERVICES.TXT"/>
    public partial class AdbClient
    {
        /// <summary>
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">
        /// The device to which to reverse forward the connections.
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to reverse forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="local">
        /// <para>
        /// The local address to reverse forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if if the specified socket is already bound through a previous reverse command.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.
        /// </returns>
        public async Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await protocol.WriteAsync($"reverse:forward:{rebind}{remote};{local}", cancellationToken).ConfigureAwait(false);

            // two adb reponses are being send:  1st OKAY is connect, 2nd OKAY is status.
            // https://android.googlesource.com/platform/system/adb/+/refs/heads/master/adb.cpp
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            if (remote.Equals("tcp:0", StringComparison.OrdinalIgnoreCase))
            {
                var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
                var data = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

                if (data != null && int.TryParse(data, out int port))
                {
                    return port;
                }
            }

            return 0;
        }

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">
        /// The device to which to forward the connections.
        /// </param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.
        /// </returns>>
        public async Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await protocol.WriteAsync($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}", cancellationToken).ConfigureAwait(false);

            // two adb reponses are being send:  1st OKAY is connect, 2nd OKAY is status.
            // https://android.googlesource.com/platform/system/adb/+/refs/heads/master/adb.cpp
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var data = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            if (data != null && int.TryParse(data, out int port))
            {
                return port;
            }

            return 0;
        }

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the port forwarding.
        /// </param>
        /// <param name="localPort">
        /// Specification of the local port that was forwarded.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.WriteAsync($"host-serial:{device.Serial}:killforward:tcp:{localPort}", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the reverse port forwarding.
        /// </param>
        /// <param name="remote">
        /// Specification of the remote that was forwarded.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);
            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"reverse:killforward:{remote}", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the port forwarding.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"host-serial:{device.Serial}:killforward-all", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Removes all reverse forwards for a given device.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove all reverse port forwarding.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);
            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"reverse:killforward-all", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="device">
        /// The device for which to list the existing forward connections.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ForwardData"/> entry for each existing forward connection.
        /// </returns>
        public async Task<IList<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"host-serial:{device.Serial}:list-forward", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var data = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            var parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(p => ForwardData.FromString(p)).ToList();
        }

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name = "device" >
        /// The device for which to list the existing reverse foward connections.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A<see cref="ForwardData"/> entry for each existing reverse forward connection.
        /// </returns>
        public async Task<IList<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken)
        {
            this.EnsureDevice(device);

            await using var protocol = await this.TryConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            await protocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await protocol.WriteAsync($"reverse:list-forward", cancellationToken).ConfigureAwait(false);
            protocol.EnsureValidAdbResponse(await protocol.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));

            var length = await protocol.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var data = await protocol.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            var parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(p => ForwardData.FromString(p)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceSocket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IPEndPoint> CreateReverseForwardAsync(DeviceData device, string deviceSocket, CancellationToken cancellationToken)
        {
            // If the port is already being forwarded, recycle that port. This prevent us from opening a new
            // socket every time.
            // This could create a problem in the following scenario:
            // - Another service have created that port.
            // - We detect that port & recycle the port.
            // - The other service closes the port while we're still using it.
            // Then again, which other service would be interested in the ports we are using on the remote device?
            // For now, we're taking the risk, but it is easy to revert back by simply deleting this block of code :-).
            var allForwards = await this.ListReverseForwardAsync(device, cancellationToken).ConfigureAwait(false);
            var existingForwards = allForwards.Where(f => deviceSocket.Equals(f.RemoteSpec.ToString(), StringComparison.OrdinalIgnoreCase));

            if (existingForwards.Count() > 1)
            {
                this.logger.LogInformation($"Multiple reverse port forwards exists for socket {deviceSocket} on device {device}");
            }

            // Taking the last if there are multiple sockets open.
            // The choise is rather arbitrary but reusing the first and only is as dangerous.
            ForwardData existingForward = existingForwards.LastOrDefault();

            var host = this.socketLocator.GetAdbSocket().Item2.Address;

            if (existingForward != null)
            {
                int recycledPort = existingForward.LocalSpec.Port;
                this.logger.LogInformation($"Recycled port forwarding for socket {deviceSocket} on device {device.Serial}; the local endpoint is {recycledPort}.");
                return new IPEndPoint(host, recycledPort);
            }

            // Find an available port
            using var portReservation = TcpPort.GetAvailablePort();
            var port = portReservation.PortNumber;

            await this.CreateReverseForwardAsync(device, deviceSocket, $"tcp:{port}", true, cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation($"Created reverse port forwarding for socket {deviceSocket} on device {device.Serial}; the local port is {port}.");

            return new IPEndPoint(host, port);
        }
    }
}
