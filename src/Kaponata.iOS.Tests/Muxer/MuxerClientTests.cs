// <copyright file="MuxerClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerClient"/> method.
    /// </summary>
    public class MuxerClientTests
    {
        /// <summary>
        /// The <see cref="MuxerClient"/> constructors validate the argments being passed.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("logger", () => new MuxerClient(null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new MuxerClient(NullLogger<MuxerClient>.Instance, null));

            Assert.Throws<ArgumentNullException>("logger", () => new MuxerClient(null, NullLoggerFactory.Instance, new MuxerSocketLocator()));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new MuxerClient(NullLogger<MuxerClient>.Instance, null, new MuxerSocketLocator()));
            Assert.Throws<ArgumentNullException>("socketLocator", () => new MuxerClient(NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance, null));
        }

        /// <summary>
        /// <see cref="MuxerClient.TryConnectToMuxerAsync(CancellationToken)"/> returns <see langword="null"/> if
        /// <see cref="MuxerSocketLocator.GetMuxerSocket"/> returns <see langword="null"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryConnectToMuxerAsync_ReturnsNull_Async()
        {
            var locator = new Mock<MuxerSocketLocator>();
            locator.Setup(l => l.GetMuxerSocket()).Returns((null, null));

            var client = new MuxerClient(NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance, locator.Object);
            Assert.Null(await client.TryConnectToMuxerAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> returns an empty list if the usbmuxd
        /// socket is not available.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_NoSocket_ReturnsEmptyList_Async()
        {
            var locator = new Mock<MuxerSocketLocator>();
            locator.Setup(l => l.GetMuxerSocket()).Returns((null, null));

            var client = new MuxerClient(NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance, locator.Object);
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Empty(result);
        }

        /// <summary>
        /// The <see cref="MuxerClient.ListDevicesAsync(CancellationToken)"/> method returns a list of devices
        /// which are currently connected to the host.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ListDevicesAsync_ReturnsDeviceList_Async()
        {
            var protocol = new Mock<MuxerProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MuxerMessage>(), default))
                .Callback<MuxerMessage, CancellationToken>((message, ct) =>
                {
                    Assert.Equal(MuxerMessageType.ListDevices, message.MessageType);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.DisposeAsync())
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(
                    DeviceListMessage.Read(
                        (NSDictionary)PropertyListParser.Parse("Muxer/devicelist-wifi.xml")));

            var clientMock = new Mock<MuxerClient>(NullLogger<MuxerClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            clientMock.Setup(c => c.TryConnectToMuxerAsync(default)).ReturnsAsync(protocol.Object);

            var client = clientMock.Object;
            var result = await client.ListDevicesAsync(default).ConfigureAwait(false);
            Assert.Collection(
                result,
                device =>
                {
                    Assert.Equal(MuxerConnectionType.Network, device.ConnectionType);
                    Assert.Equal(2, device.DeviceID);
                    Assert.Equal(IPAddress.Parse("192.168.10.239"), device.IPAddress);
                    Assert.Equal("cccccccccccccccccccccccccccccccccccccccc", device.Udid);
                });

            protocol.Verify();
        }
    }
}
