// <copyright file="AdbProtocol.Sync.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Nerdbank.Streams;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> commands for the <see cref="AdbProtocol"/> class.
    /// </summary>
    public partial class AdbProtocol
    {
        private const int MaxBufferSize = 64 * 1024;

        /// <summary>
        /// Starts a sync session.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task StartSyncSessionAsync(CancellationToken cancellationToken)
        {
            await this.WriteAsync("sync:", cancellationToken).ConfigureAwait(false);
            this.EnsureValidAdbResponse(await this.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Asynchronously writes a command to the <c>ADB</c> server.
        /// </summary>
        /// <param name="commandType">
        /// The command type to be written.
        /// </param>
        /// <param name="data">
        /// The command to be written.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task WriteSyncCommandAsync(SyncCommandType commandType, string data, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            var commandLength = AdbEncoding.GetByteCount(data);
            using var messageBuffer = this.memoryPool.Rent(commandLength + 8);

            BinaryPrimitives.WriteInt32BigEndian(messageBuffer.Memory[0..4].Span, (int)commandType);
            BinaryPrimitives.WriteUInt32LittleEndian(messageBuffer.Memory[4..8].Span, (uint)commandLength);
            AdbEncoding.GetBytes(data).CopyTo(messageBuffer.Memory[8..].Span);

            await this.stream.WriteAsync(messageBuffer.Memory.Slice(0, commandLength + 8), cancellationToken).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads a <see cref="SyncCommandType"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="SyncCommandType"/> received from from the <c>ADB</c> server.
        /// </returns>
        public virtual async Task<SyncCommandType> ReadSyncCommandTypeAsync(CancellationToken cancellationToken)
        {
            using var messageBuffer = this.memoryPool.Rent(4);

            await this.stream.ReadAsync(messageBuffer.Memory.Slice(0, 4), cancellationToken).ConfigureAwait(false);
            return (SyncCommandType)BinaryPrimitives.ReadUInt32BigEndian(messageBuffer.Memory.Span);
        }

        /// <summary>
        /// Asynchronously reads a file statistic from from the <c>ADB</c> server.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="FileStatistics"/> received from from the <c>ADB</c> server.
        /// </returns>
        public virtual async Task<FileStatistics> ReadFileStatisticsAsync(CancellationToken cancellationToken)
        {
            using var messageBuffer = this.memoryPool.Rent(16);

            await this.stream.ReadAsync(messageBuffer.Memory.Slice(0, 16), cancellationToken).ConfigureAwait(false);
            var fileStatistics = new FileStatistics()
            {
                FileMode = BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[0..4].Span),
                Size = BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[4..].Span),
                Time = DateTimeOffset.FromUnixTimeSeconds(BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[8..].Span)),
            };

            var length = BinaryPrimitives.ReadInt32LittleEndian(messageBuffer.Memory[12..].Span);
            fileStatistics.Path = await this.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            return fileStatistics;
        }

        /// <summary>
        /// Asynchronously writes a command to the <c>ADB</c> server.
        /// </summary>
        /// <param name="commandType">
        /// The command type to be written.
        /// </param>
        /// <param name="data">
        /// The data to be written.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        /// <seealso href="https://android.googlesource.com/platform/system/core.git/+/brillo-m7-dev/adb/SYNC.TXT"/>
        public virtual async Task WriteSyncCommandAsync(SyncCommandType commandType, int data, CancellationToken cancellationToken)
        {
            using var messageBuffer = this.memoryPool.Rent(8);

            BinaryPrimitives.WriteInt32BigEndian(messageBuffer.Memory[0..4].Span, (int)commandType);
            BinaryPrimitives.WriteInt32LittleEndian(messageBuffer.Memory[4..8].Span, data);

            await this.stream.WriteAsync(messageBuffer.Memory.Slice(0, 8), cancellationToken).ConfigureAwait(false);
            await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills a given buffer with bytes from the specified System.IO.Stream.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to fill from the stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous read operation. Its resulting value contains the total number of bytes read into the buffer.
        /// </returns>
        public async ValueTask<int> ReadBlockAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await this.stream.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads binary data from the adb sync service.
        /// </summary>
        /// <param name="stream">
        /// The output stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task CopySyncDataAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var syncStream = new SyncStream(this);
            await syncStream.CopyToAsync(stream).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes data to the <c>ADB</c> sync service.
        /// </summary>
        /// <param name="stream">
        /// The input stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        /// <seealso href="https://android.googlesource.com/platform/system/core.git/+/brillo-m7-dev/adb/SYNC.TXT"/>
        public virtual async Task WriteSyncDataAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var maxChunkData = MaxBufferSize - 8; // max 64k - 8 (header)

            while (true)
            {
                var bytesToWrite = Math.Min((int)(stream.Length - stream.Position), maxChunkData);

                if (bytesToWrite <= 0)
                {
                    break;
                }

                using (var buffer = this.memoryPool.Rent(bytesToWrite))
                {
                    if (await stream.ReadBlockAsync(buffer.Memory.Slice(0, bytesToWrite), cancellationToken).ConfigureAwait(false) != bytesToWrite)
                    {
                        throw new InvalidOperationException("Failed to read the stream.");
                    }

                    await this.WriteSyncCommandAsync(SyncCommandType.DATA, bytesToWrite, cancellationToken).ConfigureAwait(false);
                    await this.stream.WriteAsync(buffer.Memory.Slice(0, bytesToWrite), cancellationToken).ConfigureAwait(false);
                    await this.stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
