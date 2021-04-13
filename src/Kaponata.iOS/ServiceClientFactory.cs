// <copyright file="ServiceClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS
{
    /// <summary>
    /// A <see cref="ClientFactory{T}"/> which connects to lockdown services running on an iOS device.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the client for the lockdown service.
    /// </typeparam>
    public abstract class ServiceClientFactory<T> : ClientFactory<T>
    {
        private readonly LockdownClientFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClientFactory{T}"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can be used to create new lockdown clients.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public ServiceClientFactory(MuxerClient muxer, DeviceContext context, LockdownClientFactory lockdownClientFactory, ILogger<T> logger)
            : base(muxer, context, logger)
        {
            this.factory = lockdownClientFactory ?? throw new ArgumentNullException(nameof(lockdownClientFactory));
        }

        /// <summary>
        /// Asynchronously starts a service on the device, and returns a <see cref="Stream"/> which represents a connection
        /// to the remote service.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the lockdown service to start.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. This task returns a <see cref="Stream"/>
        /// which represents a connection to the remote service when completed.
        /// </returns>
        protected async Task<Stream> StartServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            ServiceDescriptor service;

            await using (var lockdown = await this.factory.CreateAsync(cancellationToken))
            {
                service = await lockdown.StartServiceAsync(serviceName, cancellationToken).ConfigureAwait(false);
            }

            var serviceStream = await this.Muxer.ConnectAsync(this.Context.Device, service.Port, cancellationToken);
            return serviceStream;
        }
    }
}