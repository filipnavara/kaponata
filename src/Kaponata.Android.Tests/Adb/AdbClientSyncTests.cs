// <copyright file="AdbClientSyncTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> class sync methods.
    /// </summary>
    public class AdbClientSyncTests
    {
        /// <summary>
        /// The <see cref="AdbClient.PushAsync(DeviceData, Stream, string, int, DateTimeOffset, CancellationToken)"/> pulls the data in chunks from the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Pull_PullsData_Async()
        {
            var dataStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var stream = new MemoryStream();
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.RECV, "/sdcard", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.CopySyncDataAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask)
                .Callback<Stream, CancellationToken>((s, c) => dataStream.CopyTo(s))
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;

            var timestamp = new DateTime(1980, 12, 31);
            await client.PullAsync(new DeviceData() { Serial = "123" }, "/sdcard", stream, default).ConfigureAwait(false);
            stream.Position = 0;
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());

            protocolMock.Verify();
            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.GetFileStatisticsAsync(DeviceData, string, CancellationToken)"/> method gets the file statistics of the remote file.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetFileStatisticsAsync_GetsFileStats_Async()
        {
            var dataStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var stream = new MemoryStream();
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.STAT, "/sdcard/text.txt", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.STAT)
                .Verifiable();
            protocolMock
                .Setup(p => p.ReadFileStatisticsAsync(default))
                .ReturnsAsync(new FileStatistics()
                {
                    FileMode = 456,
                    Path = "test.txt",
                    Size = 100,
                    Time = new DateTime(1980, 12, 31),
                })
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;

            var stats = await client.GetFileStatisticsAsync(new DeviceData() { Serial = "123" }, "/sdcard/text.txt", default).ConfigureAwait(false);

            Assert.Equal("/sdcard/text.txt", stats.Path);
            Assert.Equal<uint>(456, stats.FileMode);
            Assert.Equal<uint>(100, stats.Size);
            Assert.Equal(new DateTime(1980, 12, 31), stats.Time);

            protocolMock.Verify();
            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.GetFileStatisticsAsync(DeviceData, string, CancellationToken)"/> method throws on fail response.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetFileStatisticsAsync_ThrowsOnFail_Async()
        {
            var dataStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var stream = new MemoryStream();
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.STAT, "/sdcard/text.txt", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.FAIL)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;

            var exception = await Assert.ThrowsAsync<AdbException>(async () => await client.GetFileStatisticsAsync(new DeviceData() { Serial = "123" }, "/sdcard/text.txt", default).ConfigureAwait(false));
            Assert.Equal("The server returned an invalid sync response: FAIL", exception.Message);

            protocolMock.Verify();
            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.PullAsync(DeviceData, string, Stream, CancellationToken)"/> methods validates the arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Pull_ValidatesArguments_Async()
        {
            var stream = new MemoryStream();

            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;
            var timestamp = new DateTime(1980, 12, 31);
            await Assert.ThrowsAsync<ArgumentNullException>("device", async () => await client.PullAsync(null, "/sdcard", stream, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("stream", async () => await client.PullAsync(new DeviceData() { Serial = "123" }, "/sdcard", null, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.PullAsync(new DeviceData() { Serial = "123" }, null, stream, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.PullAsync(new DeviceData() { Serial = "123" }, string.Empty, stream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.PushAsync(DeviceData, Stream, string, int, DateTimeOffset, CancellationToken)"/> methods validates the arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Push_ValidatesArguments_Async()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;
            var timestamp = new DateTime(1980, 12, 31);
            await Assert.ThrowsAsync<ArgumentNullException>("device", async () => await client.PushAsync(null, stream, "/sdcard", 456, timestamp, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("stream", async () => await client.PushAsync(new DeviceData() { Serial = "123" }, null, "/sdcard", 456, timestamp, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.PushAsync(new DeviceData() { Serial = "123" }, stream, null, 456, timestamp, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.PushAsync(new DeviceData() { Serial = "123" }, stream, string.Empty, 456, timestamp, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.PushAsync(new DeviceData() { Serial = "123" }, stream, new string(new char[2000]), 456, timestamp, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.GetFileStatisticsAsync(DeviceData, string, CancellationToken)"/> methods validates the arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetFileStatisticsAsync_ValidatesArguments_Async()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;
            var timestamp = new DateTime(1980, 12, 31);
            await Assert.ThrowsAsync<ArgumentNullException>("device", async () => await client.GetFileStatisticsAsync(null, "/sdcard", default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.GetFileStatisticsAsync(new DeviceData() { Serial = "123" }, null, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>("remotePath", async () => await client.GetFileStatisticsAsync(new DeviceData() { Serial = "123" }, string.Empty, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.PushAsync(DeviceData, Stream, string, int, DateTimeOffset, CancellationToken)"/> pushes the data in chunks to the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Push_PushesData_Async()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var receivedStream = new MemoryStream();
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success)
                .Verifiable();
            protocolMock
                .Setup(p => p.WriteSyncCommandAsync(SyncCommandType.SEND, "/sdcard,456", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.WriteSyncDataAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask)
                .Callback<Stream, CancellationToken>((s, c) => s.CopyTo(receivedStream))
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock
                .Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default))
                .ReturnsAsync(protocolMock.Object)
                .Verifiable();

            var client = clientMock.Object;

            var timestamp = new DateTime(1980, 12, 31);
            await client.PushAsync(new DeviceData() { Serial = "123" }, stream, "/sdcard", 456, timestamp, default).ConfigureAwait(false);
            receivedStream.Position = 0;
            Assert.Equal(1, receivedStream.ReadByte());
            Assert.Equal(2, receivedStream.ReadByte());
            Assert.Equal(3, receivedStream.ReadByte());

            protocolMock.Verify();
            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.GetDirectoryListingAsync(DeviceData, string, System.Threading.CancellationToken)"/> method lists the files from the given directory.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetDirectoryListing_ListsFiles_Async()
        {
            var adbProtocolMock = new Mock<AdbProtocol>();

            adbProtocolMock.Setup(p => p.WriteSyncCommandAsync(SyncCommandType.LIST, "/sdcard", default)).Returns(Task.CompletedTask);
            adbProtocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.DONE);
            adbProtocolMock.SetupSequence(p => p.ReadFileStatisticsAsync(default))
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file1",
                    FileMode = 123,
                    Size = 101,
                    Time = new DateTime(1980, 12, 31),
                })
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file2",
                    FileMode = 123,
                    Size = 102,
                    Time = new DateTime(1980, 12, 31),
                });

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(adbProtocolMock.Object);
            var client = clientMock.Object;

            var listing = await client.GetDirectoryListingAsync(new DeviceData() { Serial = "123" }, "/sdcard", default).ConfigureAwait(false);

            Assert.Equal(2, listing.Count);

            var firstFile = listing.First();
            Assert.Equal("file1", firstFile.Path);
            Assert.Equal<uint>(101, firstFile.Size);

            var lastFile = listing.Last();
            Assert.Equal("file2", lastFile.Path);
            Assert.Equal<uint>(102, lastFile.Size);
        }

        /// <summary>
        /// The <see cref="AdbClient.GetDirectoryListingAsync(DeviceData, string, System.Threading.CancellationToken)"/> method throws an exception on a failure.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetDirectoryListing_ThrowsOnFail_Async()
        {
            var adbProtocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            adbProtocolMock.Setup(p => p.WriteSyncCommandAsync(SyncCommandType.LIST, "/sdcard", default)).Returns(Task.CompletedTask);
            adbProtocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.FAIL);
            adbProtocolMock.SetupSequence(p => p.ReadFileStatisticsAsync(default))
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file1",
                    FileMode = 123,
                    Size = 101,
                    Time = new DateTime(1980, 12, 31),
                })
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file2",
                    FileMode = 123,
                    Size = 102,
                    Time = new DateTime(1980, 12, 31),
                });

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.ConnectToSyncServiceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default)).ReturnsAsync(adbProtocolMock.Object);
            var client = clientMock.Object;

            await Assert.ThrowsAsync<AdbException>(async () => await client.GetDirectoryListingAsync(new DeviceData() { Serial = "123" }, "/sdcard", default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.ConnectToSyncServiceAsync(DeviceData, System.Threading.CancellationToken)"/> connects to the sync service.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ConnectToSyncService_ConnectsToSyncService_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.WriteAsync("sync:", default))
                .Verifiable();
            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success)
                .Verifiable();
            protocolMock.Setup(p => p.WriteAsync("host:transport:123", default))
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);
            var client = clientMock.Object;

            await client.ConnectToSyncServiceAsync(new DeviceData() { Serial = "123" }, default).ConfigureAwait(false);

            protocolMock.Verify();
        }
    }
}
