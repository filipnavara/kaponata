// <copyright file="AdbClientSyncTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
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
