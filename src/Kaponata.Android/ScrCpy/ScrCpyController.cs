using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Class used to send messages to the scrcpy server.
    /// </summary>
    public class ScrCpyController : IDisposable
    {
        /// <summary>
        /// The underlying stream to which control messages are being written.
        /// </summary>
        private readonly Stream stream;
        private readonly ILogger<ScrCpyController> logger;
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrCpyController"/> class.
        /// </summary>
        /// <param name="deviceInfo">
        /// The scrcpy device info.
        /// </param>
        /// <param name="stream">
        /// The stream to which scrcpy control messages can be transmitted.
        /// </param>
        /// <param name="logger">
        /// A logger to use when logging diagnostic data.
        /// </param>
        public ScrCpyController(DeviceInfo deviceInfo, Stream stream, ILogger<ScrCpyController> logger)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.DeviceInfo = deviceInfo;
        }

        /// <summary>
        /// Gets the scrcpy device info.
        /// </summary>
        public DeviceInfo DeviceInfo
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this controller is disposed.
        /// </summary>
        public bool Disposed
        {
            get
            {
                return this.disposed;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.stream.Dispose();
            this.disposed = true;
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

            // Write the message in a buffer
            var size = message.BinarySize;
            using var data = this.memoryPool.Rent(size);
            message.Write(data.Memory.Slice(0, size));

            // Send the packet
            await this.stream.WriteAsync(data.Memory.Slice(0, size), cancellationToken).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
