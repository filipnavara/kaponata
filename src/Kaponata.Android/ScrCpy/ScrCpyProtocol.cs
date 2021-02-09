// <copyright file="ScrCpyProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// The <see cref="ScrCpyProtocol"/> allows interacting with the ScrCpy server.
    /// </summary>
    public class ScrCpyProtocol : IAsyncDisposable
    {
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private readonly Stream stream;
        private readonly ILogger<ScrCpyProtocol> logger;
        private readonly bool ownsStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrCpyProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to <c>ScrCpy</c> server.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="ScrCpyProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A logger which is used to log messages.
        /// </param>
        public ScrCpyProtocol(Stream stream, bool ownsStream, ILogger<ScrCpyProtocol> logger)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ownsStream = ownsStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrCpyProtocol"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for unit testing purposes only.
        /// </remarks>
        protected ScrCpyProtocol()
            : this(Stream.Null, false, NullLogger<ScrCpyProtocol>.Instance)
        {
        }

        /// <summary>
        /// Sends a scrcpy control message.
        /// </summary>
        /// <param name="message">
        /// The message to be transmitted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public virtual async Task SendControlMessageAsync(IControlMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using var buffer = this.memoryPool.Rent(message.BinarySize);
            message.Write(buffer.Memory);

            await this.stream.WriteAsync(buffer.Memory, cancellationToken).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual ValueTask DisposeAsync()
        {
            if (this.ownsStream)
            {
                return this.stream.DisposeAsync();
            }
            else
            {
                return ValueTask.CompletedTask;
            }
        }
    }
}
