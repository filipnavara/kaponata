// <copyright file="AdbClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Provides a client interface for the Android Debug Bridge (ADB) running on the host.
    /// </summary>
    public partial class AdbClient
    {
        private readonly ILogger<AdbClient> logger;
        private readonly ILoggerFactory loggerFactory;

        private readonly AdbSocketLocator socketLocator = new AdbSocketLocator();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        public AdbClient(ILogger<AdbClient> logger, ILoggerFactory loggerFactory)
            : this(logger, loggerFactory, new AdbSocketLocator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        /// <param name="socketLocator">
        /// A <see cref="AdbSocketLocator"/> which is used to locate the <c>ADB</c> server socket.
        /// </param>
        public AdbClient(ILogger<AdbClient> logger, ILoggerFactory loggerFactory, AdbSocketLocator socketLocator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.socketLocator = socketLocator ?? throw new ArgumentNullException(nameof(socketLocator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for unit testing purposes only.
        /// </remarks>
        protected AdbClient()
            : this(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Creates a new connection to <c>ADB</c>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="AdbProtocol"/> which represents the connection to <c>ADB</c>, or <see langword="null"/> if
        /// <c>ADB</c> is not running.
        /// </returns>
        public virtual async Task<AdbProtocol> TryConnectToAdbAsync(CancellationToken cancellationToken)
        {
            var stream = await this.socketLocator.ConnectToAdbAsync(cancellationToken).ConfigureAwait(false);

            if (stream == null)
            {
                return null;
            }

            this.logger.LogInformation("A new connection is created to the ADB server.");

            return new AdbProtocol(
                stream,
                ownsStream: true,
                this.loggerFactory.CreateLogger<AdbProtocol>());
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="device"/>
        /// parameter is <see langword="null"/>, and a <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="device"/> does not have a valid serial number.
        /// </summary>
        /// <param name="device">
        /// A <see cref="DeviceData"/> object to validate.
        /// </param>
        public virtual void EnsureDevice(DeviceData device)
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
