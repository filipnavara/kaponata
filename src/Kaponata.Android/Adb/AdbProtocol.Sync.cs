// <copyright file="AdbProtocol.Sync.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the <c>ADB</c> commands for the <see cref="AdbProtocol"/> class.
    /// </summary>
    public partial class AdbProtocol
    {
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
        /// <param name="command">
        /// The command to be written.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task WriteSyncCommandAsync(SyncCommandType commandType, string command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandLength = AdbEncoding.GetByteCount(command);
            using var messageBuffer = this.memoryPool.Rent(commandLength + 8);

            BinaryPrimitives.WriteInt32BigEndian(messageBuffer.Memory[0..4].Span, (int)commandType);
            BinaryPrimitives.WriteUInt32LittleEndian(messageBuffer.Memory[4..8].Span, (uint)commandLength);
            AdbEncoding.GetBytes(command).CopyTo(messageBuffer.Memory[8..].Span);

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
                FileMode = (UnixFileMode)(BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[0..4].Span) & 0x11000),
                Size = BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[4..].Span),
                Time = DateTimeOffset.FromUnixTimeSeconds(BinaryPrimitives.ReadUInt32LittleEndian(messageBuffer.Memory[8..].Span)),
            };

            var length = BinaryPrimitives.ReadInt32LittleEndian(messageBuffer.Memory[12..].Span);
            fileStatistics.Path = await this.ReadStringAsync(length, cancellationToken).ConfigureAwait(false);

            return fileStatistics;
        }
    }
}
