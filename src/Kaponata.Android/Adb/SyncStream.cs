// <copyright file="SyncStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// The <see cref="SyncStream"/> wraps the <c>ADB</c> server stream and used to read/write sync data.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/core.git/+/brillo-m7-dev/adb/SYNC.TXT"/>
    public class SyncStream : Stream
    {
        private const int MaxBufferSize = 64 * 1024;
        private readonly AdbProtocol adbProtocol;
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

        private SyncCommandType syncCommandType = 0;
        private int chunckPosition = 0;
        private int chunckSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncStream"/> class.
        /// </summary>
        /// <param name="adbProtocol">
        /// The <c>ADB</c> server stream.
        /// </param>
        public SyncStream(AdbProtocol adbProtocol)
        {
            this.adbProtocol = adbProtocol;
        }

        /// <inheritdoc/>
        public override bool CanRead { get; } = true;

        /// <inheritdoc/>
        public override bool CanSeek { get; } = false;

        /// <inheritdoc/>
        public override bool CanWrite { get; } = true;

        /// <inheritdoc/>
        public override long Length => throw new System.NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        /// <inheritdoc/>
        public override void Flush()
        {
            throw new System.NotSupportedException();
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            using var lengthBuffer = this.memoryPool.Rent(4);

            // command type, reading chunck, reading chunck size
            switch (this.syncCommandType, this.chunckPosition < this.chunckSize, this.chunckSize == 0)
            {
                case (SyncCommandType.DONE, _, _):
                    return 0;
                case (SyncCommandType.FAIL, _, _):
                    var message = await this.adbProtocol.ReadIndefiniteLengthStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new AdbException($"Failed to pull. {message}");
                case (0, _, _):
                case (_, false, false):
                    this.syncCommandType = await this.adbProtocol.ReadSyncCommandTypeAsync(cancellationToken).ConfigureAwait(false);
                    this.chunckPosition = 0;
                    this.chunckSize = 0;
                    return await this.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                case (_, _, _) when this.syncCommandType != SyncCommandType.DATA:
                    throw new AdbException($"The server sent an invalid response {this.syncCommandType}");
                case (SyncCommandType.DATA, true, false):
                    var length = Math.Min(buffer.Length, this.chunckSize - this.chunckPosition);
                    int read;

                    if ((read = await this.adbProtocol.ReadBlockAsync(buffer[0..length], cancellationToken).ConfigureAwait(false)) != length)
                    {
                        throw new AdbException("Failed to read the stream.");
                    }

                    this.chunckPosition = this.chunckPosition + read;
                    return read;
                case (SyncCommandType.DATA, false, true):

                    if (await this.adbProtocol.ReadBlockAsync(lengthBuffer.Memory[0..4], cancellationToken).ConfigureAwait(false) != 4)
                    {
                        throw new AdbException("Failed to read the stream.");
                    }

                    this.chunckSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(lengthBuffer.Memory[0..4].Span);
                    if (this.chunckSize > MaxBufferSize - 4)
                    {
                        throw new AdbException($"The adb server is sending {this.chunckSize} bytes of data, which exceeds the maximum chunk size {MaxBufferSize - 4}");
                    }

                    this.chunckPosition = 0;

                    return await this.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                default:
                    throw new AdbException($"The server sent an invalid response {this.syncCommandType}");
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
            throw new System.NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException();
        }
    }
}
