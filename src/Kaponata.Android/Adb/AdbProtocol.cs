// <copyright file="AdbProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// The <see cref="AdbProtocol"/> allows interacting with the Android Debug Bridge (ADB) using a <see cref="Stream"/>.
    /// </summary>
    public class AdbProtocol : IAsyncDisposable
    {
        /// <summary>
        /// The default encoding.
        /// </summary>
        public const string DefaultEncoding = "ISO-8859-1";

        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private readonly Stream stream;
        private readonly ILogger<AdbProtocol> logger;
        private readonly bool ownsStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to <c>ADB</c>.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="AdbProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A logger which is used to log messages.
        /// </param>
        public AdbProtocol(Stream stream, bool ownsStream, ILogger<AdbProtocol> logger)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ownsStream = ownsStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbProtocol"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for unit testing purposes only.
        /// </remarks>
        protected AdbProtocol()
            : this(Stream.Null, false, NullLogger<AdbProtocol>.Instance)
        {
        }

        /// <summary>
        /// Gets the encoding used when communicating with adb.
        /// </summary>
        public static Encoding AdbEncoding { get; } = Encoding.GetEncoding(DefaultEncoding);

        /// <summary>
        /// Switches the connection to the device/emulator identified by <paramref name="device"/>.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrEmpty(device.Serial))
            {
                throw new ArgumentException(nameof(device));
            }

            await this.WriteAsync($"host:transport:{device.Serial}", cancellationToken).ConfigureAwait(false);
            this.EnsureValidAdbResponse(await this.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Reads the <c>ADB</c> response.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// An <see cref="AdbResponse"/> indicating whether the <c>ADB</c> server responded OKAY or FAIL.
        /// </returns>
        public virtual async Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken)
        {
            var status = await this.ReadAdbResponseStatusAsync(cancellationToken).ConfigureAwait(false);

            if (status == AdbResponseStatus.OKAY)
            {
                return AdbResponse.Success;
            }
            else
            {
                var length = await this.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
                var message = await this.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);
                return new AdbResponse(AdbResponseStatus.FAIL, message);
            }
        }

        /// <summary>
        /// Reads an <c>ADB</c> response status.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// An <see cref="AdbResponse"/> indicating whether the <c>ADB</c> server responded OKAY or FAIL.
        /// </returns>s
        public virtual async Task<AdbResponseStatus> ReadAdbResponseStatusAsync(CancellationToken cancellationToken)
        {
            using var messageBuffer = this.memoryPool.Rent(4);

            await this.stream.ReadAsync(messageBuffer.Memory.Slice(0, 4), cancellationToken).ConfigureAwait(false);
            return (AdbResponseStatus)BinaryPrimitives.ReadInt32BigEndian(messageBuffer.Memory.Span);
        }

        /// <summary>
        /// Ensures a valid <see cref="AdbResponse"/>.
        /// This method throw an exception when the response contains <see cref="AdbResponseStatus.FAIL"/>.
        /// </summary>
        /// <param name="response">
        /// The <see cref="AdbResponse"/> to be validated.
        /// </param>
        public virtual void EnsureValidAdbResponse(AdbResponse response)
        {
            if (response.Status != AdbResponseStatus.OKAY)
            {
                throw new InvalidDataException(response.Message);
            }
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

        /// <summary>
        /// Asynchronously reads an <see cref="int"/> from from the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> received from from the <c>ADB</c> server.
        /// </returns>
        public virtual async Task<ushort> ReadUInt16Async(CancellationToken cancellationToken)
        {
            using var messageBuffer = this.memoryPool.Rent(4);
            using var intBuffer = this.memoryPool.Rent(2);

            if (await this.stream.ReadBlockAsync(messageBuffer.Memory.Slice(0, 4), cancellationToken).ConfigureAwait(false) != 4)
            {
                throw new InvalidOperationException("Failed to read an integer.");
            }

            return HexPrimitives.ReadUShort(messageBuffer.Memory.Span);
        }

        /// <summary>
        ///  Asynchronously reads a <see cref="string"/> from the <c>ADB</c> server.
        /// </summary>
        /// <param name="length">
        /// The length of the <see cref="string"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> received from the <c>ADB</c> server.
        /// </returns>
        public virtual async Task<string> ReadStringAsync(int length, CancellationToken cancellationToken)
        {
            int read;

            using var messageBuffer = this.memoryPool.Rent(length);
            if ((read = await this.stream.ReadBlockAsync(messageBuffer.Memory.Slice(0, length), cancellationToken).ConfigureAwait(false)) != length)
            {
                this.logger.LogInformation("Could only read {read}/{total} bytes; exiting.", read);
                return null;
            }

            return AdbEncoding.GetString(messageBuffer.Memory.Slice(0, length).Span);
        }

        /// <summary>
        ///  Asynchronously reads a <see cref="string"/> with indefinite length from the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> received from the <c>ADB</c> server.
        /// </returns>
        public virtual async Task<string> ReadIndefiniteLengthStringAsync(CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(this.stream, AdbEncoding);

            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the content of the stream  to the <c>ADB</c> server.
        /// </summary>
        /// <param name="stream">
        /// The data stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task WriteAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            await stream.CopyToAsync(this.stream).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a message to the <c>ADB</c> server.
        /// </summary>
        /// <param name="message">
        /// The message to write.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task WriteAsync(string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            ushort length = (ushort)AdbEncoding.GetByteCount(message);
            using var messageBuffer = this.memoryPool.Rent(length + 4);

            HexPrimitives.WriteUInt16(length, messageBuffer.Memory[0..].Span);
            AdbEncoding.GetBytes(message).CopyTo(messageBuffer.Memory[4..].Span);

            await this.stream.WriteAsync(messageBuffer.Memory.Slice(0, length + 4), cancellationToken).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
