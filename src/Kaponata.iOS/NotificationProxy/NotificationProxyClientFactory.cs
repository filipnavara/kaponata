// <copyright file="NotificationProxyClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.NotificationProxy
{
    /// <summary>
    /// A <see cref="ServiceClientFactory{T}"/> which can create <see cref="NotificationProxyClient"/> clients.
    /// </summary>
    public class NotificationProxyClientFactory : ServiceClientFactory<NotificationProxyClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyClientFactory"/> class.
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
        public NotificationProxyClientFactory(MuxerClient muxer, DeviceContext context, ClientFactory<LockdownClient> lockdownClientFactory, ILogger<NotificationProxyClient> logger)
            : base(muxer, context, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<NotificationProxyClient> CreateAsync(CancellationToken cancellationToken)
            => this.CreateAsync(NotificationProxyClient.ServiceName, cancellationToken);

        /// <summary>
        /// Asynchronously creates a new instance of the <see cref="NotificationProxyClient"/> client.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the
        /// newly created <see cref="NotificationProxyClient"/> service client when completed.
        /// </returns>
        public async Task<NotificationProxyClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var serviceStream = await this.StartServiceAsync(serviceName, cancellationToken);
            return new NotificationProxyClient(serviceStream, this.Logger);
        }
    }
}
