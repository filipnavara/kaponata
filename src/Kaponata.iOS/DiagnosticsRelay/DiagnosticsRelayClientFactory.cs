// <copyright file="DiagnosticsRelayClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Creates connections to the <see cref="DiagnosticsRelay"/> service running on an iOS device.
    /// </summary>
    public class DiagnosticsRelayClientFactory : ServiceClientFactory<DiagnosticsRelayClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRelayClientFactory"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockdown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public DiagnosticsRelayClientFactory(MuxerClient muxer, DeviceContext context, ClientFactory<LockdownClient> lockdownClientFactory, ILogger<DiagnosticsRelayClient> logger)
            : base(muxer, context, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<DiagnosticsRelayClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(DiagnosticsRelayClient.ServiceName, cancellationToken);

        /// <inheritdoc/>
        public override async Task<DiagnosticsRelayClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var serviceStream = await this.StartServiceAsync(serviceName, cancellationToken);
            return new DiagnosticsRelayClient(serviceStream, this.Logger);
        }
    }
}
