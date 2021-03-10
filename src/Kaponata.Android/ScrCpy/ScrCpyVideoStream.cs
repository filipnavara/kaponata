// <copyright file="ScrCpyVideoStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// A <see cref="Stream"/> which allows reading the H.264 stream exposed by scrcpy.
    /// </summary>
    public class ScrCpyVideoStream : Stream
    {
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

        /// <summary>
        /// The underlying stream from which data is being read.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// The logger to which to log.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The header for the current packet.
        /// </summary>
        private PacketHeader? packetHeader;

        /// <summary>
        /// The number of bytes left in the current packet.
        /// </summary>
        private int bytesLeftInPacket = 0;

        /// <summary>
        /// A value indicating whether the current packet is the first packet.
        /// </summary>
        private bool firstPacket = true;
        private int packetNr = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrCpyVideoStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The underlying stream to which represents the raw network connection to the scrcpy
        /// server.
        /// </param>
        /// <param name="logger">
        /// A logger to use when logging diagnostic data.
        /// </param>
        public ScrCpyVideoStream(Stream stream, ILogger<ScrCpyVideoStream> logger)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the scrcpy device info.
        /// </summary>
        public ScrCpyDeviceInfo DeviceInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Reads the scrcpy device info.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The device info.
        /// </returns>
        public async Task<ScrCpyDeviceInfo> ReadDeviceInfoAsync(CancellationToken cancellationToken)
        {
            this.logger.LogDebug("Reading device info");
            using var buffer = this.memoryPool.Rent(ScrCpyDeviceInfo.BinarySize);
            if (await this.stream.ReadBlockAsync(buffer.Memory[0..ScrCpyDeviceInfo.BinarySize], cancellationToken).ConfigureAwait(false) != ScrCpyDeviceInfo.BinarySize)
            {
                throw new InvalidOperationException("Failed to read the stream.");
            }

            return ScrCpyDeviceInfo.Read(buffer.Memory[0..ScrCpyDeviceInfo.BinarySize].Span);
        }

        /// <summary>
        /// Reads a scrcpy packet header.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The packet header.
        /// </returns>
        public async Task<PacketHeader> ReadPacketHeaderAsync(CancellationToken cancellationToken)
        {
            this.logger.LogDebug("Reading device info");
            using var buffer = this.memoryPool.Rent(PacketHeader.BinarySize);
            if (await this.stream.ReadBlockAsync(buffer.Memory[0..PacketHeader.BinarySize], cancellationToken).ConfigureAwait(false) != PacketHeader.BinarySize)
            {
                throw new InvalidOperationException("Failed to read the stream.");
            }

            return PacketHeader.Read(buffer.Memory[0..PacketHeader.BinarySize].Span);
        }

        /// <inheritdoc/>
        public override void Flush() => throw new NotSupportedException();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            if (this.DeviceInfo == null)
            {
                this.DeviceInfo = await this.ReadDeviceInfoAsync(cancellationToken).ConfigureAwait(false);
            }

            if (this.bytesLeftInPacket == 0)
            {
                this.logger.LogDebug("Reading packet header");
                this.packetHeader = await this.ReadPacketHeaderAsync(cancellationToken).ConfigureAwait(false);

                if (this.packetHeader == null)
                {
                    this.logger.LogWarning("Failed to read packet header. Exiting.");
                    return 0;
                }

                if (this.firstPacket)
                {
                    if (this.packetHeader.Value.PacketTimeStamp != ulong.MaxValue)
                    {
                        throw new InvalidDataException("The timestamp of the first packet is invalid. Is the data corrupt?");
                    }

                    this.firstPacket = false;
                }

                this.bytesLeftInPacket = (int)this.packetHeader.Value.PacketLength;
            }

            int bytesToRead = Math.Min(memory.Length, this.bytesLeftInPacket);
            int bytesRead = await this.stream.ReadAsync(memory[0..bytesToRead], cancellationToken).ConfigureAwait(false);
            this.bytesLeftInPacket -= bytesRead;

            this.logger.LogDebug($"Read {bytesRead}/{memory.Length} bytes. Bytes left in packet {this.bytesLeftInPacket}.");
            return bytesRead;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}