﻿// <copyright file="ServiceClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.PropertyLists;
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
        private readonly ClientFactory<LockdownClient> factory;
        private readonly PropertyListProtocolFactory propertyListProtocolFactory;

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
        /// <param name="propertyListProtocolFactory">
        /// A <see cref="PropertyListProtocolFactory"/> which can be used to create new instances of the <see cref="PropertyListProtocol"/> class.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can be used to create new lockdown clients.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public ServiceClientFactory(MuxerClient muxer, DeviceContext context, PropertyListProtocolFactory propertyListProtocolFactory, ClientFactory<LockdownClient> lockdownClientFactory, ILogger<T> logger)
            : base(muxer, context, logger)
        {
            this.factory = lockdownClientFactory ?? throw new ArgumentNullException(nameof(lockdownClientFactory));
            this.propertyListProtocolFactory = propertyListProtocolFactory ?? throw new ArgumentNullException(nameof(propertyListProtocolFactory));
        }

        /// <summary>
        /// Asynchronously starts a service on the device, and returns a <see cref="Stream"/> which represents a connection
        /// to the remote service.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the lockdown service to start.
        /// </param>
        /// <param name="startSession">
        /// A value indicating whether to start a session before starting the service. This is required for most services;
        /// with some exceptions (such as the unsecured notification service).
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. This task returns a <see cref="Stream"/>
        /// which represents a connection to the remote service when completed.
        /// </returns>
        protected async Task<PropertyListProtocol> StartServiceAsync(string serviceName, bool startSession, CancellationToken cancellationToken)
        {
            ServiceDescriptor service;

            await using (var lockdown = await this.factory.CreateAsync(cancellationToken))
            {
                StartSessionResponse session = null;

                if (startSession)
                {
                    session = await lockdown.StartSessionAsync(this.Context.PairingRecord, cancellationToken).ConfigureAwait(false);
                }

                service = await lockdown.StartServiceAsync(serviceName, cancellationToken).ConfigureAwait(false);

                if (session != null)
                {
                    await lockdown.StopSessionAsync(session.SessionID, cancellationToken).ConfigureAwait(false);
                }
            }

            var serviceStream = await this.Muxer.ConnectAsync(this.Context.Device, service.Port, cancellationToken);
            var protocol = this.propertyListProtocolFactory.Create(serviceStream, ownsStream: true, this.Logger);

            if (service.EnableServiceSSL)
            {
                await protocol.EnableSslAsync(this.Context.PairingRecord, cancellationToken).ConfigureAwait(false);
            }

            return protocol;
        }
    }
}
