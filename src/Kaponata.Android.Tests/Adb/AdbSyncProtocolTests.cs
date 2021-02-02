// <copyright file="AdbSyncProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        /// The <see cref="AdbProtocol.WriteSyncDataAsync(Stream, System.Threading.CancellationToken)"/> validates the argument.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteSyncDataAsync_ValidatesArguments_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.WriteSyncDataAsync((Stream)null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, System.Threading.CancellationToken)"/> validates the argument.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadSyncDataAsync_ValidatesArguments_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.CopySyncDataAsync((Stream)null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, CancellationToken)"/> method reads the stream data in chuncks from the sync service.
        /// </summary>
        /// <param name="size">
        /// The data size.
        /// </param>
        /// <param name="syncCommandTypes">
        /// The sync reponses.
        /// </param>
        /// <param name="expectedChunkSizes">
        /// The expected chunk sizes.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(
            (64 * 1024) + 100,
            new SyncCommandType[] { SyncCommandType.DATA, SyncCommandType.DATA, SyncCommandType.DONE },
            new int[] { 65528, 108 })]
        [InlineData(
            100,
            new SyncCommandType[] { SyncCommandType.DATA, SyncCommandType.DONE },
            new int[] { 100 })]
        public async Task ReadSyncData_ReadsData_Async(int size, SyncCommandType[] syncCommandTypes, int[] expectedChunkSizes)
        {
            // prepare devicestream
            var data = new byte[size + (expectedChunkSizes.Length * 4)];
            var expected = new byte[size];
            var position = 0;
            var positionExpected = 0;
            foreach (int chunksize in expectedChunkSizes)
            {
                var lengthBuffer = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, chunksize);
                lengthBuffer.CopyTo(data, position);
                position = position + 4;

                var chunk = new byte[chunksize];
                var random = new Random();
                random.NextBytes(chunk);

                chunk.CopyTo(expected, positionExpected);
                chunk.CopyTo(data, position);
                position = position + chunksize;
                positionExpected = positionExpected + chunksize;
            }

            using var dataStream = new MemoryStream();
            using var deviceStream = new MemoryStream(data);

            var chunkSizes = new List<int>();
            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            var readCommandTypeSequence = protocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default));
            syncCommandTypes.ToList().ForEach(c => readCommandTypeSequence.ReturnsAsync(c));

            var protocol = protocolMock.Object;
            await protocol.CopySyncDataAsync(dataStream, default).ConfigureAwait(false);
            dataStream.Position = 0;

            expected.ToList().ForEach(b => Assert.Equal(b, dataStream.ReadByte()));
            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, CancellationToken)"/> method throws an exception when there is an error in the data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadSyncData_ReadsDataError_Async()
        {
            // prepare devicestream
            var data = new byte[90 + 4];
            var lengthBuffer = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, 100);
            lengthBuffer.CopyTo(data, 0);

            using var dataStream = new MemoryStream();
            using var deviceStream = new MemoryStream(data);

            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            protocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DATA)
                .ReturnsAsync(SyncCommandType.DONE);

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<AdbException>(async () => await protocol.CopySyncDataAsync(dataStream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, CancellationToken)"/> method throws an exception when there is an error in the data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadSyncData_ReadsDataMaxLengthError_Async()
        {
            // prepare devicestream
            var data = new byte[90 + 4];
            var lengthBuffer = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, 64 * 1024 * 2);
            lengthBuffer.CopyTo(data, 0);

            using var dataStream = new MemoryStream();
            using var deviceStream = new MemoryStream(data);

            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            protocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DATA)
                .ReturnsAsync(SyncCommandType.DONE);

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<AdbException>(async () => await protocol.CopySyncDataAsync(dataStream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, CancellationToken)"/> method throws an exception when there is an error in the data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadSyncData_ReadsDataLengthError_Async()
        {
            // prepare devicestream
            var data = new byte[3];

            using var dataStream = new MemoryStream();
            using var deviceStream = new MemoryStream(data);

            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            protocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DATA)
                .ReturnsAsync(SyncCommandType.DONE);

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<AdbException>(async () => await protocol.CopySyncDataAsync(dataStream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.CopySyncDataAsync(Stream, CancellationToken)"/> method throws an exception when an unexpected command type is received.
        /// </summary>
        /// <param name="response">
        /// The invalid response.
        /// </param>
        /// <param name="exceptionMessage">
        /// The expected exception message.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(SyncCommandType.STAT, "The server sent an invalid response STAT")]
        [InlineData(SyncCommandType.FAIL, "Failed to pull. ")]
        public async Task ReadSyncData_ThrowsOnUnexpected_Async(SyncCommandType response, string exceptionMessage)
        {
            using var dataStream = new MemoryStream();
            using var deviceStream = new MemoryStream();

            var chunkSizes = new List<int>();
            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            var readCommandTypeSequence = protocolMock
                .Setup(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(response);

            var protocol = protocolMock.Object;
            var exception = await Assert.ThrowsAsync<AdbException>(async () => await protocol.CopySyncDataAsync(dataStream, default).ConfigureAwait(false));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteSyncDataAsync(Stream, CancellationToken)"/> method throws an exception when reading from the input stream fails.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteSyncData_WritesDataError_Async()
        {
            var dataStream = new Mock<Stream>();
            dataStream
                .SetupGet(s => s.Length).Returns(300);
            dataStream
                .SetupGet(s => s.Position).Returns(0);

            using var deviceStream = new MemoryStream();

            var chunkSizes = new List<int>();
            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };
            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.DATA, It.IsAny<int>(), default)).Returns(Task.CompletedTask)
                .Callback<SyncCommandType, int, CancellationToken>((t, l, c) =>
                {
                    chunkSizes.Add(l);
                })
                .Verifiable();

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await protocol.WriteSyncDataAsync(dataStream.Object, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteSyncDataAsync(Stream, CancellationToken)"/> method writes the stream data in chuncks to the sync service.
        /// </summary>
        /// <param name="size">
        /// The data size.
        /// </param>
        /// <param name="expectedChunkSizes">
        /// The expected chunk sizes.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData((64 * 1024) + 100, new int[] { 65528, 108 })]
        [InlineData(100, new int[] { 100 })]
        public async Task WriteSyncData_WritesData_Async(int size, int[] expectedChunkSizes)
        {
            var data = new byte[size];
            var random = new Random();
            random.NextBytes(data);

            using var dataStream = new MemoryStream(data);
            using var deviceStream = new MemoryStream();

            var chunkSizes = new List<int>();
            var protocolMock = new Mock<AdbProtocol>(deviceStream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };
            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.DATA, It.IsAny<int>(), default)).Returns(Task.CompletedTask)
                .Callback<SyncCommandType, int, CancellationToken>((t, l, c) =>
                {
                    chunkSizes.Add(l);
                })
                .Verifiable();

            var protocol = protocolMock.Object;
            await protocol.WriteSyncDataAsync(dataStream, default).ConfigureAwait(false);
            Assert.Equal(expectedChunkSizes, chunkSizes);
            protocolMock.Verify();
        }

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
        /// The <see cref="AdbProtocol.WriteSyncCommandAsync(SyncCommandType, int, System.Threading.CancellationToken)"/> method writes the command with int data to the sync service.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteSyncCommand_WritesCommandInt_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await protocol.WriteSyncCommandAsync(SyncCommandType.SEND, 100, default).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var output = new List<byte>() { (byte)'S', (byte)'E', (byte)'N', (byte)'D', 100, 0, 0, 0 };
            Assert.Equal(output.Count, stream.Length);
            output.ForEach(b => Assert.Equal(b, stream.ReadByte()));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteSyncCommandAsync(SyncCommandType, string, System.Threading.CancellationToken)"/> method validates the argument.
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
