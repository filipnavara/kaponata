// <copyright file="PropertyListProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.PropertyLists
{
    /// <summary>
    /// The <see cref="PropertyListProtocol"/> supports reading and writing messages in property list format.
    /// </summary>
    public partial class PropertyListProtocol : IAsyncDisposable
    {
        private readonly Stream rawStream;
        private readonly bool ownsStream;
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private readonly ILogger logger;

        private Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="PropertyListProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public PropertyListProtocol(Stream stream, bool ownsStream, ILogger logger)
        {
            this.rawStream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.stream = this.rawStream;
            this.ownsStream = ownsStream;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListProtocol"/> class.
        /// Intended for unit testing purposes only.
        /// </summary>
        protected PropertyListProtocol()
        {
        }

        /// <summary>
        /// Gets the <see cref="Stream"/> which is used to communicate with the device.
        /// </summary>
        public Stream Stream => this.stream;

        /// <summary>
        /// Asynchronously sends a message to the remote lockdown client.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task WriteMessageAsync(NSDictionary message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Serialize the underlying message so we can calculate the packet size
            var xml = message.ToXmlPropertyList();

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Sending data:\r\n{data}", xml);
            }

            int messageLength = Encoding.UTF8.GetByteCount(xml);

            var packetLength = 4 + messageLength;

            using (var packet = this.memoryPool.Rent(packetLength))
            {
                // Construct the entire packet:
                // [length] (4 bytes)
                // [UTF-8 XML-encoded property list message] (N bytes)
                BinaryPrimitives.WriteInt32BigEndian(packet.Memory.Span[0..4], messageLength);

                Encoding.UTF8.GetBytes(xml, packet.Memory.Span[4.. (messageLength + 4)]);

                // Send the packet
                await this.stream.WriteAsync(packet.Memory[0..packetLength], cancellationToken).ConfigureAwait(false);
                await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously reads a lockdown message from the stream.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="byte"/> array containing the message data when available; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        public virtual async Task<NSDictionary> ReadMessageAsync(CancellationToken cancellationToken)
        {
            int length;

            using (var lengthBuffer = this.memoryPool.Rent(4))
            {
                if (await this.stream.ReadBlockAsync(lengthBuffer.Memory[0..4], cancellationToken).ConfigureAwait(false) != 4)
                {
                    return null;
                }

                length = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer.Memory[0..4].Span);
            }

            using (var messageBuffer = this.memoryPool.Rent(length))
            {
                if (await this.stream.ReadBlockAsync(messageBuffer.Memory[0..length], cancellationToken).ConfigureAwait(false) != length)
                {
                    return null;
                }

                var dict = (NSDictionary)PropertyListParser.Parse(messageBuffer.Memory[0..length].Span);

                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    this.logger.LogTrace("Recieving data:\r\n{data}", dict.ToXmlPropertyList());
                }

                return dict;
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (this.stream != this.rawStream)
            {
                await this.stream.DisposeAsync().ConfigureAwait(false);
            }

            if (this.rawStream != null)
            {
                await this.rawStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
