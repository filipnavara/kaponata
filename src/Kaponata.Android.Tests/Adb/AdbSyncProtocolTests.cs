// <copyright file="AdbSyncProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbProtocol"/> sync method.
    /// </summary>
    public class AdbSyncProtocolTests
    {
        /// <summary>
        /// The <see cref="AdbProtocol.ReadSyncCommandTypeAsync(System.Threading.CancellationToken)"/> reads a <see cref="SyncCommandType"/>.
        /// </summary>
        /// <param name="data">
        /// The <c>ADB</c> server data.
        /// </param>
        /// <param name="result">
        /// The expected <see cref="SyncCommandType"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("DATA", SyncCommandType.DATA)]
        [InlineData("DENT", SyncCommandType.DENT)]
        [InlineData("DONE", SyncCommandType.DONE)]
        [InlineData("FAIL", SyncCommandType.FAIL)]
        [InlineData("LIST", SyncCommandType.LIST)]
        [InlineData("OKAY", SyncCommandType.OKAY)]
        [InlineData("RECV", SyncCommandType.RECV)]
        [InlineData("SEND", SyncCommandType.SEND)]
        [InlineData("STAT", SyncCommandType.STAT)]
        public async Task ReadSyncCommandType_ReadsType_Async(string data, SyncCommandType result)
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(data).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            var type = await protocol.ReadSyncCommandTypeAsync(default).ConfigureAwait(false);

            Assert.Equal(result, type);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteSyncCommandAsync(SyncCommandType, string, System.Threading.CancellationToken)"/> writes the command.
        /// </summary>
        /// <param name="command">
        /// The invallid command.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task WriteSyncCommand_ValidatesArguments_Async(string command)
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.WriteSyncCommandAsync(SyncCommandType.SEND, command, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteSyncCommandAsync(SyncCommandType, string, System.Threading.CancellationToken)"/> writes the command.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteSyncCommand_WritesCommand_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await protocol.WriteSyncCommandAsync(SyncCommandType.SEND, "/test", default).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var output = new List<byte>() { (byte)'S', (byte)'E', (byte)'N', (byte)'D', 5, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            Assert.Equal(output.Count, stream.Length);
            output.ForEach(b => Assert.Equal(b, stream.ReadByte()));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.StartSyncSessionAsync(System.Threading.CancellationToken)"/> methods starts the sync session.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task StartSyncSession_StartsSyncSession_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.WriteAsync("sync:", default))
                    .Verifiable();
            protocolMock
                    .Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success);

            var protocol = protocolMock.Object;
            await protocol.StartSyncSessionAsync(default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbProtocol.StartSyncSessionAsync(System.Threading.CancellationToken)"/> throws exception the <c>ADB</c> server responds with FAIL.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task StartSyncSessionAsync_ThrowsWhenStartFails_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.WriteAsync("sync:", default))
                    .Verifiable();
            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(new AdbResponse()
                {
                    Status = AdbResponseStatus.FAIL,
                    Message = "Oesje!",
                })
                .Verifiable();

            var protocol = protocolMock.Object;
            var exception = await Assert.ThrowsAsync<AdbException>(async () => await protocol.StartSyncSessionAsync(default).ConfigureAwait(false));
            Assert.Equal("Oesje!", exception.Message);
            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadFileStatisticsAsync(System.Threading.CancellationToken)"/> method reads the file statistics.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadFileStatistics__Async()
        {
            var statsData = new byte[] { 249, 65, 0, 0, 0, 16, 0, 0, 34, 183, 8, 92, 7, 0, 0, 0, (byte)'S', (byte)'a', (byte)'m', (byte)'s', (byte)'u', (byte)'n', (byte)'g' };
            MemoryStream stream = new MemoryStream(statsData, true);

            stream.Position = 0;

            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            var stats = await protocol.ReadFileStatisticsAsync(default).ConfigureAwait(false);

            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1544075042), stats.Time);

            Assert.Equal<uint>(4096, stats.Size);
            Assert.Equal("Samsung", stats.Path);
            Assert.Equal<uint>(16889, stats.FileMode);
            Assert.Equal("Samsung", stats.ToString());
        }
    }
}
