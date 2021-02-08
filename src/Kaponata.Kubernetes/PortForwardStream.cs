// <copyright file="PortForwardStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Provides read-write access to a port on a Kubernetes pod, using the port-forward protocol.
    /// </summary>
    public class PortForwardStream : Stream
    {
        private readonly WebSocket webSocket;
        private readonly ILogger<PortForwardStream> logger;

        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private bool streamsInitialized = false;
        private bool newMessage = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortForwardStream"/> class.
        /// </summary>
        /// <param name="webSocket">
        /// A <see cref="WebSocket"/> which represents the connection to the remote port.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        public PortForwardStream(WebSocket webSocket, ILogger<PortForwardStream> logger)
        {
            this.webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the underlying <see cref="WebSocket"/> which is used when communicating with the remote port.
        /// </summary>
        public WebSocket WebSocket
        {
            get { return this.webSocket; }
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // First byte contains the stream index
            // Byte two and three the port number
            if (!this.streamsInitialized)
            {
                this.logger.LogInformation("Initializing stream. Reading first packets.");

                var tempBuffer = new byte[3];
                var result = await this.webSocket.ReceiveAsync(tempBuffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                var port = BinaryPrimitives.ReadInt16LittleEndian(tempBuffer.AsSpan(1, 2));
                this.logger.LogInformation("Initializing stream: read value {port} on stream {stream}.", port, tempBuffer[0]);

                Debug.Assert(tempBuffer[0] == 0, "The first data packet must relate to stream 0");

                result = await this.webSocket.ReceiveAsync(tempBuffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                port = BinaryPrimitives.ReadInt16LittleEndian(tempBuffer.AsSpan(1, 2));
                this.logger.LogInformation("Initializing stream: read value {port} on stream {stream}.", port, tempBuffer[0]);

                Debug.Assert(tempBuffer[0] == 1, "The second data packet must relate to stream 1");

                this.streamsInitialized = true;

                this.logger.LogInformation("Successfully intialized stream.");
            }

            if (!this.newMessage)
            {
                var result = await this.webSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                this.newMessage = result.EndOfMessage;

                this.logger.LogInformation("Read {read}/{count} bytes of a message continuation.", result.Count, buffer.Length);
                return result.Count;
            }
            else
            {
                using (var memory = this.memoryPool.Rent(buffer.Length + 1))
                {
                    memory.Memory.Span.Fill(0);
                    var result = await this.webSocket.ReceiveAsync(memory.Memory.Slice(0, buffer.Length + 1), cancellationToken).ConfigureAwait(false);
                    this.newMessage = result.EndOfMessage;

                    var index = memory.Memory.Span[0];
                    Debug.Assert(index == 0 || index == 1, "Any data packet must relate to stream 0 or 1");

                    if (index == 0)
                    {
                        memory.Memory.Slice(1, result.Count - 1).CopyTo(buffer);
                        this.logger.LogInformation("Read {read}/{count} bytes of a message continuation.", result.Count, buffer.Length);
                        return result.Count - 1;
                    }
                    else
                    {
                        this.logger.LogInformation("Got data on stream {stream}.", index);
                        var message = Encoding.UTF8.GetString(memory.Memory.Slice(1, result.Count - 1).Span);
                        throw new IOException(message);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            using (var memory = this.memoryPool.Rent(buffer.Length + 1))
            {
                memory.Memory.Span[0] = 0;
                buffer.CopyTo(memory.Memory.Slice(1, buffer.Length));

                await this.webSocket.SendAsync(memory.Memory.Slice(0, buffer.Length + 1), WebSocketMessageType.Binary, endOfMessage: false, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.webSocket.Dispose();
        }
    }
}
