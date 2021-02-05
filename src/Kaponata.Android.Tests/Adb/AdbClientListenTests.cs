// <copyright file="AdbClientListenTests.cs" company="Quamotion bv">
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
    /// Tests the <see cref="AdbClient.ListenAsync(System.Func{List{DeviceData}, Task{AdbListenAction}}, CancellationToken)"/> method.
    /// </summary>
    public class AdbClientListenTests
    {
        private const string Device1 = @"0100a9ee51a18f2b device product:bullhead model:Nexus_5X device:bullhead features:shell_v2,cmd";
        private const string Device2 = @"EAOKCY112414           device usb:1-1 product:WW_K013 model:K013 device:K013_1";

        /// <summary>
        /// <see cref="AdbClient.ListenAsync"/> returns <see langword="false"/> when adb is unavailable.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_NoAdb_ReturnsFalse_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync((AdbProtocol)null);
            var client = clientMock.Object;

            Assert.False(await client.ListenAsync((onDeviceData) => { return Task.FromResult<AdbListenAction>(AdbListenAction.ContinueListening); }, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="AdbClient.ListenAsync"/> throws when no callback is provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_NoCallBack_Throws_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>("onNewDeviceList", async () =>
                  await client.ListenAsync(
                      null,
                      default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="AdbClient.ListenAsync"/> loops through all messages until the adb closes the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_ReadAll_Async()
        {
            var events = new Queue<List<DeviceData>>();
            var queue = new Queue<string>(
                new string[]
                {
                    Device1,
                    Device1,
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    null,
                });

            var protocol = new Mock<AdbProtocol>();
            protocol.Setup(p => p.ReadUInt16Async(CancellationToken.None)).ReturnsAsync((ushort)12);
            protocol.Setup(p => p.ReadStringAsync(12, CancellationToken.None)).ReturnsAsync(queue.Dequeue);
            protocol.Setup(p => p.WriteAsync("host:track-devices", CancellationToken.None)).Returns(Task.CompletedTask);
            protocol.Setup(p => p.ReadAdbResponseAsync(CancellationToken.None)).ReturnsAsync(AdbResponse.Success);

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.False(
                await client.ListenAsync(
                    (onDeviceData) =>
                    {
                        events.Enqueue(onDeviceData);
                        return Task.FromResult(AdbListenAction.ContinueListening);
                    },
                    default).ConfigureAwait(false));

            Assert.Empty(queue);
            Assert.Equal(new int[] { 1, 1, 2, 2, 2 }, events.Select(e => e.Count));
            protocol.Verify();
        }

        /// <summary>
        /// <see cref="AdbClient.ListenAsync"/> stops looping when the on attached callback instructs it to do so.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListenAsync_StopsOnAttached_Async()
        {
            var events = new Queue<List<DeviceData>>();
            var queue = new Queue<string>(
                new string[]
                {
                    Device1,
                    Device1,
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    string.Join("\r\n", new string[] { Device1, Device2 }),
                    null,
                });

            var protocol = new Mock<AdbProtocol>();
            protocol.Setup(p => p.ReadUInt16Async(CancellationToken.None)).ReturnsAsync((ushort)12);
            protocol.Setup(p => p.ReadStringAsync(12, CancellationToken.None)).ReturnsAsync(queue.Dequeue);
            protocol.Setup(p => p.WriteAsync("host:track-devices", CancellationToken.None)).Returns(Task.CompletedTask);
            protocol.Setup(p => p.ReadAdbResponseAsync(CancellationToken.None)).ReturnsAsync(AdbResponse.Success);

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.True(
                await client.ListenAsync(
                    (onDeviceData) =>
                    {
                        events.Enqueue(onDeviceData);
                        return Task.FromResult(onDeviceData.Count == 1 ? AdbListenAction.ContinueListening : AdbListenAction.StopListening);
                    },
                    default).ConfigureAwait(false));

            Assert.Equal(3, queue.Count);
            Assert.Equal(new int[] { 1, 1, 2 }, events.Select(e => e.Count));
            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.ReadDeviceEventAsync(AdbProtocol, CancellationToken)"/> reads an event.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ReadDeviceEvent_ReadsEvent_Async()
        {
            var protocol = new Mock<AdbProtocol>();

            protocol.Setup(p => p.ReadUInt16Async(CancellationToken.None)).ReturnsAsync((ushort)12);
            protocol.Setup(p => p.ReadStringAsync(12, CancellationToken.None)).ReturnsAsync(Device2);
            protocol.Setup(p => p.ReadAdbResponseAsync(CancellationToken.None)).ReturnsAsync(AdbResponse.Success);

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            var deviceEvent = await client.ReadDeviceEventAsync(protocol.Object, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(Device2, deviceEvent);
        }

        /// <summary>
        /// The <see cref="AdbClient.ReadDeviceEventAsync(AdbProtocol, CancellationToken)"/> returns null on zero length.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ReadDeviceEvent_ReturnsNullOnZeroLength_Async()
        {
            var protocol = new Mock<AdbProtocol>();

            protocol.Setup(p => p.ReadUInt16Async(CancellationToken.None)).ReturnsAsync((ushort)0);
            protocol.Setup(p => p.ReadAdbResponseAsync(CancellationToken.None)).ReturnsAsync(AdbResponse.Success);

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            var deviceEvent = await client.ReadDeviceEventAsync(protocol.Object, CancellationToken.None).ConfigureAwait(false);
            Assert.Null(deviceEvent);
        }
    }
}
