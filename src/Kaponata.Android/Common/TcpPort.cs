// <copyright file="TcpPort.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Provides static methods for interacting with TCP ports.
    /// </summary>
    public static class TcpPort
    {
        /// <summary>
        /// A <see cref="Semaphore"/> which makes sure no two threads would get the same port number.
        /// </summary>
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        /// <summary>
        /// Gets an available TCP port.
        /// </summary>
        /// <returns>
        /// A <see cref="TcpPortReservation"/> which locks the current port to the calling thread. As long as you
        /// hold the reservation, subsequent threads cannot.
        /// </returns>
        public static TcpPortReservation GetAvailablePort()
        {
            // Find an available port
            Semaphore.Wait();

            // This is not thread safe -- two threads entering at the same time would get the same port number,
            // but they cannot both open a TCP socket on that port.
            // So we use a semaphore to make sure only one thread at a time can request a new number. It is the
            // responsibility of the calling thread to release the semaphore (by disposing the TcpPortReservation)
            // once they have started using the port, and a new call to TcpClistener would return a new port
            // number.
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            return new TcpPortReservation(port, Semaphore);
        }
    }
}
