// <copyright file="MuxerClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.Muxer
{
    /// <summary>
    /// Provides a client interface for the Apple Mobile Device Multiplexor (usbmuxd) running on the host.
    /// </summary>
    public partial class MuxerClient
    {
        private readonly ILogger<MuxerClient> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly MuxerSocketLocator socketLocator = new MuxerSocketLocator();

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerClient"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        public MuxerClient(ILogger<MuxerClient> logger, ILoggerFactory loggerFactory)
            : this(logger, loggerFactory, new MuxerSocketLocator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerClient"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        /// <param name="socketLocator">
        /// A <see cref="MuxerSocketLocator"/> which is used to locate the usbmuxd socket.
        /// </param>
        public MuxerClient(ILogger<MuxerClient> logger, ILoggerFactory loggerFactory, MuxerSocketLocator socketLocator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.socketLocator = socketLocator ?? throw new ArgumentNullException(nameof(socketLocator));
        }

        /// <summary>
        /// Creates a new connection to <c>usbmuxd</c>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="MuxerProtocol"/> which represents the connection to <c>usbmuxd</c>, or <see langword="null"/> if
        /// usbmuxd is not running.
        /// </returns>
        public virtual async Task<MuxerProtocol> TryConnectToMuxerAsync(CancellationToken cancellationToken)
        {
            (Socket socket, EndPoint endPoint) = this.socketLocator.GetMuxerSocket();

            if (socket == null || endPoint == null)
            {
                return null;
            }

            await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);

            return new MuxerProtocol(
                new NetworkStream(socket, ownsSocket: true),
                ownsStream: true,
                this.loggerFactory.CreateLogger<MuxerProtocol>());
        }
    }
}
