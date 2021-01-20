// <copyright file="AdbClientCommandsTest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the commands <see cref="AdbClient"/> class.
    /// </summary>
    public class AdbClientCommandsTest
    {
        /// <summary>
        /// The <see cref="AdbClient.GetDevicesAsync(System.Threading.CancellationToken)"/> method returns the connected devices.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetDevices_ReturnsDevicesList_Async()
        {
            var protocol = new Mock<AdbProtocol>();
            protocol.Setup(p => p.WriteAsync("host:devices-l", default))
                    .Verifiable();
            protocol.Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success)
                    .Verifiable();
            protocol.Setup(p => p.ReadUInt16Async(default))
                .ReturnsAsync((ushort)95)
                .Verifiable();
            protocol.Setup(p => p.ReadStringAsync(95, default))
                    .ReturnsAsync("ce11182b2bcb852c02     device product:dreamltexx model:SM_G950F device:dreamlte transport_id:4")
                    .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            var device = Assert.Single(await client.GetDevicesAsync(default).ConfigureAwait(false));
            Assert.Equal(string.Empty, device.Features);
            Assert.Equal(string.Empty, device.Message);
            Assert.Equal("SM_G950F", device.Model);
            Assert.Equal("dreamlte", device.Name);
            Assert.Equal("dreamltexx", device.Product);
            Assert.Equal("ce11182b2bcb852c02", device.Serial);
            Assert.Equal(ConnectionState.Device, device.State);
            Assert.Equal("4", device.TransportId);
            Assert.Equal(string.Empty, device.Usb);

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.GetAdbVersionAsync(System.Threading.CancellationToken)"/> method returns the <c>ADB</c> server version.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetAdbVersion_ReturnsAdbVersion_Async()
        {
            var protocol = new Mock<AdbProtocol>();
            protocol.Setup(p => p.WriteAsync("host:version", default))
                    .Verifiable();
            protocol.Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success)
                .Verifiable();
            protocol.Setup(p => p.ReadUInt16Async(default))
                .ReturnsAsync((ushort)4)
                .Verifiable();
            protocol
                .Setup(p => p.ReadStringAsync(4, default))
                .ReturnsAsync("0029")
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            Assert.Equal(41, await client.GetAdbVersionAsync(default).ConfigureAwait(false));

            protocol.Verify();
        }
    }
}
