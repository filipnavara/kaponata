// <copyright file="SyncServiceTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="SyncService"/> class.
    /// </summary>
    public class SyncServiceTests
    {
        /// <summary>
        /// The <see cref="SyncService.GetDirectoryListingAsync(string, System.Threading.CancellationToken)"/> method lists the files from the given directory.
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
                    FileMode = UnixFileMode.Regular,
                    Size = 101,
                    Time = new DateTime(1980, 12, 31),
                })
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file2",
                    FileMode = UnixFileMode.Regular,
                    Size = 102,
                    Time = new DateTime(1980, 12, 31),
                });

            var service = new SyncService(NullLogger<SyncService>.Instance, NullLoggerFactory.Instance, adbProtocolMock.Object);
            var listing = await service.GetDirectoryListingAsync("/sdcard", default).ConfigureAwait(false);

            Assert.Equal(2, listing.Count);

            var firstFile = listing.First();
            Assert.Equal("file1", firstFile.Path);
            Assert.Equal<uint>(101, firstFile.Size);

            var lastFile = listing.Last();
            Assert.Equal("file2", lastFile.Path);
            Assert.Equal<uint>(102, lastFile.Size);
        }

        /// <summary>
        /// The <see cref="SyncService.GetDirectoryListingAsync(string, System.Threading.CancellationToken)"/> method throws an exception on a failure.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task GetDirectoryListing_ThrowsOnFail_Async()
        {
            var adbProtocolMock = new Mock<AdbProtocol>();
            adbProtocolMock.Setup(p => p.WriteSyncCommandAsync(SyncCommandType.LIST, "/sdcard", default)).Returns(Task.CompletedTask);
            adbProtocolMock.SetupSequence(p => p.ReadSyncCommandTypeAsync(default))
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.DENT)
                .ReturnsAsync(SyncCommandType.FAIL);
            adbProtocolMock.SetupSequence(p => p.ReadFileStatisticsAsync(default))
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file1",
                    FileMode = UnixFileMode.Regular,
                    Size = 101,
                    Time = new DateTime(1980, 12, 31),
                })
                .ReturnsAsync(new FileStatistics()
                {
                    Path = "file2",
                    FileMode = UnixFileMode.Regular,
                    Size = 102,
                    Time = new DateTime(1980, 12, 31),
                });

            var service = new SyncService(NullLogger<SyncService>.Instance, NullLoggerFactory.Instance, adbProtocolMock.Object);
            await Assert.ThrowsAsync<AdbException>(async () => await service.GetDirectoryListingAsync("/sdcard", default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="SyncService.ConnectAsync(DeviceData, System.Threading.CancellationToken)"/> method connects to the sync service.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ConnectAsync_ConnectsToSyncService_Async()
        {
            var adbProtocolMock = new Mock<AdbProtocol>();
            adbProtocolMock.Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default)).Returns(Task.CompletedTask).Verifiable();
            adbProtocolMock.Setup(p => p.StartSyncSessionAsync(default)).Returns(Task.CompletedTask).Verifiable();

            var service = new SyncService(NullLogger<SyncService>.Instance, NullLoggerFactory.Instance, adbProtocolMock.Object);

            Assert.False(service.Connected);
            Assert.Throws<InvalidOperationException>(service.EnsureConnected);
            await service.ConnectAsync(new DeviceData() { Serial = "123" }, default).ConfigureAwait(false);
            service.EnsureConnected();
            Assert.True(service.Connected);

            adbProtocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="SyncService.SyncService(Microsoft.Extensions.Logging.ILogger{SyncService}, Microsoft.Extensions.Logging.ILoggerFactory, AdbProtocol)"/> constructor validates arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            var adbProtocolMock = new Mock<AdbProtocol>();

            Assert.Throws<ArgumentNullException>("logger", () => new SyncService(null, NullLoggerFactory.Instance, adbProtocolMock.Object));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new SyncService(NullLogger<SyncService>.Instance, null, adbProtocolMock.Object));
            Assert.Throws<ArgumentNullException>("protocol", () => new SyncService(NullLogger<SyncService>.Instance, NullLoggerFactory.Instance, null));
        }

        /// <summary>
        /// The <see cref="SyncService.DisposeAsync"/> disposes the <see cref="AdbProtocol"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DisposeAsync_DisposesTheProtocol_Async()
        {
            var adbProtocolMock = new Mock<AdbProtocol>();
            adbProtocolMock.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            var adbProtocol = adbProtocolMock.Object;
            await using (SyncService service = new SyncService(NullLogger<SyncService>.Instance, NullLoggerFactory.Instance, adbProtocol))
            {
            }

            adbProtocolMock.Verify();
        }
    }
}
