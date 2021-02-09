// <copyright file="TcpPortReservation.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Threading;

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Represents a TCP reservation.
    /// </summary>
    public class TcpPortReservation : IDisposable
    {
        private readonly SemaphoreSlim semaphore;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpPortReservation"/> class.
        /// </summary>
        /// <param name="portNumber">
        /// The port number of the TCP port which is currently reserved.
        /// </param>
        /// <param name="semaphore">
        /// The semaphore which protects other threads from acquiring the same TCP port.
        /// </param>
        public TcpPortReservation(int portNumber, SemaphoreSlim semaphore)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            this.semaphore = semaphore;
            this.PortNumber = portNumber;
        }

        /// <summary>
        /// Gets the port number of the TCP port which is currently reserved.
        /// </summary>
        public int PortNumber
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.semaphore.Release();
        }
    }
}
