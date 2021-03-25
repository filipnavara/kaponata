// <copyright file="LockdownClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
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
        private const int LockdownPort = 0xF27E;

        private readonly Stream stream;
        private readonly LockdownProtocol protocol;
        private readonly MuxerClient muxer;
        private readonly MuxerDevice device;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the lockdown client.
        /// </param>
        /// <param name="muxer">
        /// The muxer through which we are connected.
        /// </param>
        /// <param name="device">
        /// The device on which lockdown is running.
        /// </param>
        public LockdownClient(Stream stream, MuxerClient muxer, MuxerDevice device)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.muxer = muxer ?? throw new ArgumentNullException(nameof(muxer));
            this.device = device ?? throw new ArgumentNullException(nameof(device));

            this.protocol = new LockdownProtocol(stream, ownsStream: true);
        }

        /// <summary>
        /// Gets or sets the label to use when sending or receiving messages.
        /// </summary>
        public string Label
        { get; set; } = ThisAssembly.AssemblyName;

        /// <summary>
        /// Creates a new connection to the lockdown client.
        /// </summary>
        /// <param name="muxer">
        /// The muxer which owns the connection.
        /// </param>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous oepration and returns the <see cref="LockdownClient"/> once
        /// connected.
        /// </returns>
        public static async Task<LockdownClient> ConnectAsync(MuxerClient muxer, MuxerDevice device, CancellationToken cancellationToken)
        {
            if (muxer == null)
            {
                throw new ArgumentNullException(nameof(muxer));
            }

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var stream = await muxer.ConnectAsync(device, LockdownPort, cancellationToken).ConfigureAwait(false);

            LockdownClient client = new LockdownClient(stream, muxer, device);

            // Make sure we are really connected to lockdown
            var type = await client.QueryTypeAsync(cancellationToken).ConfigureAwait(false);

            if (type != "com.apple.mobile.lockdown")
            {
                throw new InvalidOperationException();
            }

            return client;
        }

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
            return this.stream.DisposeAsync();
        }
    }
}
