// <copyright file="AdbClientCommandsTest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading;
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
        /// The <see cref="AdbClient.GetDevicesAsync(System.Threading.CancellationToken)"/> method throws an exception when connecting to the <c>ADB</c> server failed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetDevicesAsync_ThrowsWhenNoConnection_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync((AdbProtocol)null);
            var client = clientMock.Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetDevicesAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method pushes and installs the apk to the <c>ADB</c> server in case where no arguments are provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task Install_ApkPushedWithoutArguments_Async()
        {
            Stream receivedStream = null;
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocol.Setup(p => p.WriteAsync("exec:cmd package 'install' -S 3", default))
                        .Verifiable();

            protocol
                .Setup(p => p.ReadIndefiniteLengthStringAsync(default))
                .ReturnsAsync("Success\n");
            protocol.Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success)
                    .Verifiable();
            protocol
                .Setup(p => p.SetDeviceAsync(It.IsAny<DeviceData>(), default)).Returns(Task.CompletedTask);
            protocol
                .Setup(p => p.WriteAsync(It.IsAny<Stream>(), default))
                .Callback<Stream, CancellationToken>((s, c) =>
                {
                    receivedStream = s;
                });

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            using var apkStream = new MemoryStream(new byte[] { 1, 2, 3 });

            await client.InstallAsync(
                new DeviceData() { Serial = "123" },
                apkStream,
                default,
                Array.Empty<string>()).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method pushes and installs the apk to the <c>ADB</c> server.
        /// </summary>
        /// <param name="args">
        /// The arguments used to install the app.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Theory]
        [InlineData(null)]
        [InlineData("arg1")]
        [InlineData("arg1", "arg2")]
        public async Task Install_ApkPushed_Async(params string[] args)
        {
            Stream receivedStream = null;
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            if (args == null)
            {
                protocol.Setup(p => p.WriteAsync("exec:cmd package 'install' -S 3", default))
                        .Verifiable();
            }
            else
            {
                protocol.Setup(p => p.WriteAsync($"exec:cmd package 'install' {string.Join(" ", args)} -S 3", default))
                        .Verifiable();
            }

            protocol
                .Setup(p => p.ReadIndefiniteLengthStringAsync(default))
                .ReturnsAsync("Success\n");
            protocol.Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success)
                    .Verifiable();
            protocol
                .Setup(p => p.SetDeviceAsync(It.IsAny<DeviceData>(), default)).Returns(Task.CompletedTask);
            protocol
                .Setup(p => p.WriteAsync(It.IsAny<Stream>(), default))
                .Callback<Stream, CancellationToken>((s, c) =>
                {
                    receivedStream = s;
                });

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            using var apkStream = new MemoryStream(new byte[] { 1, 2, 3 });

            await client.InstallAsync(
                new DeviceData() { Serial = "123" },
                apkStream,
                default,
                args).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method throws when the <c>ADB</c> server reports a failure.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task Install_ThrowsOnFailure_Async()
        {
            Stream receivedStream = null;
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocol.Setup(p => p.WriteAsync($"exec:cmd package 'install' -S 3", default))
                    .Verifiable();

            protocol
                .Setup(p => p.ReadIndefiniteLengthStringAsync(default))
                .ReturnsAsync("Fail\n");
            protocol.Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success)
                    .Verifiable();
            protocol
                .Setup(p => p.SetDeviceAsync(It.IsAny<DeviceData>(), default)).Returns(Task.CompletedTask);
            protocol
                .Setup(p => p.WriteAsync(It.IsAny<Stream>(), default))
                .Callback<Stream, CancellationToken>((s, c) =>
                {
                    receivedStream = s;
                });

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            using var apkStream = new MemoryStream(new byte[] { 1, 2, 3 });

            var exception = await Assert.ThrowsAsync<AdbException>(async () => await client.InstallAsync(
                new DeviceData() { Serial = "123" },
                apkStream,
                default,
                null).ConfigureAwait(false));

            Assert.Equal("Fail\n", exception.Message);

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method throws when invalid device data is provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task Install_InvalidDeviceData_Async()
        {
            using var apkStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.InstallAsync(null, apkStream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method throws when an invalid serial is provided.
        /// </summary>
        /// <param name="serial">
        /// The invalid serial.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Install_InvalidDeviceSerial_Async(string serial)
        {
            using var apkStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.InstallAsync(new DeviceData() { Serial = serial }, apkStream, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method throws when a null stream is provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task Install_NullStream_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };
            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.InstallAsync(new DeviceData() { Serial = "123" }, null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.InstallAsync(DeviceData, Stream, CancellationToken, string[])"/> method throws when an invalid stream is provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task Install_InvalidStream_Async()
        {
            var apkStreamMock = new Mock<Stream>()
            {
                CallBase = true,
            };
            apkStreamMock.SetupGet(s => s.CanRead).Returns(false);

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.InstallAsync(new DeviceData() { Serial = "123" }, apkStreamMock.Object, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbClient.GetDevicesAsync(System.Threading.CancellationToken)"/> method returns the connected devices.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetDevices_ReturnsDevicesList_Async()
        {
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

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
        /// The <see cref="AdbClient.GetAdbVersionAsync(System.Threading.CancellationToken)"/> method throws an exception when connecting to the <c>ADB</c> server failed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetAdbVersion_ThrowsWhenNoConnection_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync((AdbProtocol)null);
            var client = clientMock.Object;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetAdbVersionAsync(default).ConfigureAwait(false));
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
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

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

        /// <summary>
        /// The <see cref="AdbClient.GetAdbVersionAsync(System.Threading.CancellationToken)"/> method throws when an invalid version number message is returned.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task GetAdbVersion_ThrowsOnInvalidData_Async()
        {
            var protocol = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

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
                .ReturnsAsync("0029bis")
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocol.Object);
            var client = clientMock.Object;

            var exception = await Assert.ThrowsAsync<AdbException>(async () => await client.GetAdbVersionAsync(default).ConfigureAwait(false));
            Assert.Contains("0029bis", exception.Message);
        }
    }
}
