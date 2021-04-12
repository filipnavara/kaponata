﻿// <copyright file="LockdownClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// The <see cref="LockdownClient"/> allows you to interact with the Lockdown daemon running on an iOS device.
    /// </summary>
    public partial class LockdownClient : IAsyncDisposable
    {
        /// <summary>
        /// The port on which lockdown listens.
        /// </summary>
        public const int LockdownPort = 0xF27E;

        private readonly LockdownProtocol protocol;

        private readonly ILogger<LockdownClient> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the lockdown client.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public LockdownClient(Stream stream, ILogger<LockdownClient> logger)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.protocol = new LockdownProtocol(stream, ownsStream: true, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="LockdownProtocol"/> which represents the connection to the lockdown client.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public LockdownClient(LockdownProtocol protocol, ILogger<LockdownClient> logger)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the label to use when sending or receiving messages.
        /// </summary>
        public string Label
        { get; set; } = ThisAssembly.AssemblyName;

        /// <summary>
        /// Queries the type of the connection. Used to validate this is a valid lockdown connection.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and returns the device response.
        /// </returns>
        public async Task<string> QueryTypeAsync(CancellationToken cancellationToken)
        {
            await this.protocol.WriteMessageAsync(
                new LockdownMessage()
                {
                    Label = this.Label,
                    Request = "QueryType",
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            var message = LockdownResponse<string>.Read(response);

            return message.Type;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.protocol.DisposeAsync();
        }

        /// <summary>
        /// Throws an exception when a <see cref="LockdownResponse{T}"/> indicates an error.
        /// </summary>
        /// <typeparam name="T">
        /// The result type.
        /// </typeparam>
        /// <param name="response">
        /// The response from the server.
        /// </param>
        protected void EnsureSuccess<T>(LockdownResponse<T> response)
        {
            if (response.Error != null)
            {
                throw new LockdownException($"The request failed: {response.Error}");
            }
        }
    }
}
