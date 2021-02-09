// <copyright file="AdbSocketLocator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Provides methods for connecting to the Android Debug Bridge (ADB).
    /// </summary>
    public class AdbSocketLocator
    {
        /// <summary>
        /// The name of the ADB_SOCKET_ADDRESS environment variable, which can be used to
        /// override the socket address.
        /// </summary>
        public const string SocketAddressEnvironmentVariable = "ADB_SOCKET_ADDRESS";

        /// <summary>
        /// The port at which the <c>ADB</c> server is listening.
        /// </summary>
        public const int DefaultAdbPort = 5037;

        /// <summary>
        /// The default IP address at which the Adb is listening.
        /// </summary>
        public const long DefaultAdbHost = 0x0100007f;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocketLocator"/> class.
        /// </summary>
        public AdbSocketLocator()
        {
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> and <see cref="EndPoint"/> which can be used to connect to the USB Multiplexor daemon.
        /// </summary>
        /// <returns>
        /// A <see cref="AdbProtocol"/> which represents the connection to <c>ADB</c>, or <see langword="null"/> if
        /// <c>ADB</c> server is not running.
        /// </returns>
        public virtual (Socket, IPEndPoint) GetAdbSocket()
        {
            Socket socket;
            IPEndPoint endPoint;

            var socketAddress = this.GetSocketAddressEnvironmentVariable();

            if (socketAddress != null)
            {
                var separator = socketAddress.IndexOf(':');
                var host = IPAddress.Parse(socketAddress.Substring(0, separator));
                var port = int.Parse(socketAddress[(separator + 1)..]);

                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(host, port);
            }
            else
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(DefaultAdbHost, DefaultAdbPort);
            }

            return (socket, endPoint);
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> which represents a connection to the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public virtual async Task<Stream> ConnectToAdbAsync(CancellationToken cancellationToken)
        {
            (var socket, var endPoint) = this.GetAdbSocket();

            if (socket == null)
            {
                return null;
            }

            await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);

            return new NetworkStream(socket, ownsSocket: true);
        }

        /// <summary>
        /// Gets the value of the <c>ADB_SOCKET_ADDRESS</c> environment variable.
        /// </summary>
        /// <returns>
        /// The value of the <c>ADB_SOCKET_ADDRESS</c> environment variable.
        /// </returns>
        public virtual string GetSocketAddressEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable(SocketAddressEnvironmentVariable);
        }
    }
}
