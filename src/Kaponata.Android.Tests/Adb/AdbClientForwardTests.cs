// <copyright file="AdbClientForwardTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> forward methods.
    /// </summary>
    public class AdbClientForwardTests
    {
        /// <summary>
        /// The forward methods verify the device argument.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ForwardMethods_EnsureDevice_Async()
        {
            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>())).Throws(new Exception("device not valid."));

            var client = clientMock.Object;
            var exception = await Assert.ThrowsAsync<Exception>(() => client.CreateForwardAsync(new DeviceData(), "local", "remote", true, default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.CreateReverseForwardAsync(new DeviceData(), "remote", "local", true, default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.ListForwardAsync(new DeviceData(), default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.ListReverseForwardAsync(new DeviceData(), default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.RemoveAllForwardsAsync(new DeviceData(), default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.RemoveAllReverseForwardsAsync(new DeviceData(), default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.RemoveForwardAsync(new DeviceData(), 0, default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);

            exception = await Assert.ThrowsAsync<Exception>(() => client.RemoveReverseForwardAsync(new DeviceData(), "remote", default)).ConfigureAwait(false);
            Assert.Equal("device not valid.", exception.Message);
        }

        /// <summary>
        /// The <see cref="AdbClient.CreateReverseForwardAsync(DeviceData, string, string, bool, System.Threading.CancellationToken)"/> method creates the reverse forward.
        /// </summary>
        /// <param name="allowRebind">
        /// A value indicating whether the forward should allow rebind.
        /// </param>
        /// <param name="local">
        /// The local forward specification.
        /// </param>
        /// <param name="remote">
        /// The remote forward specification.
        /// </param>
        /// <param name="expectedCommand">
        /// The expected write command.
        /// </param>
        /// <param name="adbPortResponse">
        /// The response containting the port in string format.
        /// </param>
        /// <param name="expectedPort">
        /// The expected port.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Theory]
        [InlineData(true, "tcp:1", "tcp:2", "reverse:forward:tcp:1;tcp:2", "100", 100)]
        [InlineData(false, "tcp:1", "tcp:2", "reverse:forward:norebind:tcp:1;tcp:2", "100", 100)]
        [InlineData(false, "tcp:1", "tcp:2", "reverse:forward:norebind:tcp:1;tcp:2", null, 0)]
        [InlineData(false, "tcp:1", "tcp:2", "reverse:forward:norebind:tcp:1;tcp:2", "abd", 0)]
        public async Task CreateReverseForward_CreatesReverseForward_Async(bool allowRebind, string local, string remote, string expectedCommand, string adbPortResponse, int expectedPort)
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.ReadAdbResponseAsync(default)).ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync(expectedCommand, default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock.Setup(p => p.ReadUInt16Async(default))
                 .ReturnsAsync((ushort)3)
                 .Verifiable();
            protocolMock
                .Setup(p => p.ReadStringAsync(3, default))
                .ReturnsAsync(adbPortResponse)
                .Verifiable();
            protocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "1234"), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            var port = await client.CreateReverseForwardAsync(new DeviceData() { Serial = "1234" }, remote, local, allowRebind, default).ConfigureAwait(false);
            Assert.Equal(expectedPort, port);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.CreateForwardAsync(DeviceData, string, string, bool, System.Threading.CancellationToken)"/> method creates the forward.
        /// </summary>
        /// <param name="allowRebind">
        /// A value indicating whether the forward should allow rebind.
        /// </param>
        /// <param name="expectedCommand">
        /// The expected write command.
        /// </param>
        /// <param name="adbPortResponse">
        /// The response containting the port in string format.
        /// </param>
        /// <param name="expectedPort">
        /// The expected port.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Theory]
        [InlineData(true, "host-serial:1234:forward:tcp:1;tcp:2", "100", 100)]
        [InlineData(false, "host-serial:1234:forward:norebind:tcp:1;tcp:2", "100", 100)]
        [InlineData(false, "host-serial:1234:forward:norebind:tcp:1;tcp:2", null, 0)]
        [InlineData(false, "host-serial:1234:forward:norebind:tcp:1;tcp:2", "abd", 0)]
        public async Task CreateForward_CreatesForward_Async(bool allowRebind, string expectedCommand, string adbPortResponse, int expectedPort)
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync(expectedCommand, default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock.Setup(p => p.ReadUInt16Async(default))
                 .ReturnsAsync((ushort)3)
                 .Verifiable();
            protocolMock
                .Setup(p => p.ReadStringAsync(3, default))
                .ReturnsAsync(adbPortResponse)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            var port = await client.CreateForwardAsync(new DeviceData() { Serial = "1234" }, "tcp:1", "tcp:2", allowRebind, default).ConfigureAwait(false);
            Assert.Equal(expectedPort, port);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.RemoveForwardAsync(DeviceData, int, System.Threading.CancellationToken)"/> method removes the specified forward.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task RemoveForward_RemovesForward_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("host-serial:1234:killforward:tcp:1", default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            await client.RemoveForwardAsync(new DeviceData() { Serial = "1234" }, 1, default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.RemoveReverseForwardAsync(DeviceData, string, System.Threading.CancellationToken)"/> method removes the specified reverse forward.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task RemoveReverseForwardAsync_RemovesReverseForward_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("reverse:killforward:tcp:1", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "1234"), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            await client.RemoveReverseForwardAsync(new DeviceData() { Serial = "1234" }, "tcp:1", default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.RemoveAllForwardsAsync(DeviceData, System.Threading.CancellationToken)"/> method removes all forwards.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task RemoveAllForwards_RemovesAllForwards_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("host-serial:1234:killforward-all", default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            await client.RemoveAllForwardsAsync(new DeviceData() { Serial = "1234" }, default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.RemoveAllReverseForwardsAsync(DeviceData, System.Threading.CancellationToken)"/> method removes all reverse forwards.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task RemoveAllReverseForwards_RemovesReverseForwards_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("reverse:killforward-all", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "1234"), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            await client.RemoveAllReverseForwardsAsync(new DeviceData() { Serial = "1234" }, default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.ListForwardAsync(DeviceData, System.Threading.CancellationToken)"/> method lists all forwards.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListForwards_ListsAllForwards_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("host-serial:1234:list-forward", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock.Setup(p => p.ReadUInt16Async(default))
                 .ReturnsAsync((ushort)3)
                 .Verifiable();
            protocolMock
                .Setup(p => p.ReadStringAsync(3, default))
                .ReturnsAsync("169.254.109.177:5555 tcp:1 tcp:2\n169.254.109.177:5555 tcp:3 tcp:4\n169.254.109.177:5555 tcp:5 localabstract:socket1\n")
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            var forwards = await client.ListForwardAsync(new DeviceData() { Serial = "1234" }, default).ConfigureAwait(false);
            Assert.Equal(3, forwards.Count);

            Assert.Equal(ForwardSpec.Parse("tcp:1"), forwards[0].LocalSpec);
            Assert.Equal(ForwardSpec.Parse("tcp:2"), forwards[0].RemoteSpec);
            Assert.Equal("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.Equal(ForwardSpec.Parse("tcp:3"), forwards[1].LocalSpec);
            Assert.Equal(ForwardSpec.Parse("tcp:4"), forwards[1].RemoteSpec);
            Assert.Equal("169.254.109.177:5555", forwards[1].SerialNumber);
            Assert.Equal(ForwardSpec.Parse("tcp:5"), forwards[2].LocalSpec);
            Assert.Equal(ForwardSpec.Parse("localabstract:socket1"), forwards[2].RemoteSpec);
            Assert.Equal("169.254.109.177:5555", forwards[2].SerialNumber);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.ListReverseForwardAsync(DeviceData, System.Threading.CancellationToken)"/> method lists all reverse forwards.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchrounous test.
        /// </returns>
        [Fact]
        public async Task ListReverseForwards_ListsAllReverseForwards_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success);
            protocolMock
                .Setup(p => p.WriteAsync("reverse:list-forward", default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "1234"), default))
                .Returns(Task.CompletedTask)
                .Verifiable();
            protocolMock.Setup(p => p.ReadUInt16Async(default))
                 .ReturnsAsync((ushort)3)
                 .Verifiable();
            protocolMock
                .Setup(p => p.ReadStringAsync(3, default))
                .ReturnsAsync("(reverse) localabstract:scrcpy tcp:100\n(reverse) localabstract:scrcpy2 tcp:100\n(reverse) localabstract:scrcpy3 tcp:100\n")
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);
            clientMock.Setup(c => c.EnsureDevice(It.IsAny<DeviceData>()));
            clientMock.Setup(c => c.TryConnectToAdbAsync(default)).ReturnsAsync(protocolMock.Object);

            var client = clientMock.Object;
            var forwards = await client.ListReverseForwardAsync(new DeviceData() { Serial = "1234" }, default).ConfigureAwait(false);

            Assert.Equal(3, forwards.Count);

            Assert.Equal(ForwardSpec.Parse("localabstract:scrcpy"), forwards[0].RemoteSpec);
            Assert.Equal(ForwardSpec.Parse("tcp:100"), forwards[0].LocalSpec);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal(ForwardSpec.Parse("tcp:100"), forwards[1].LocalSpec);
            Assert.Equal(ForwardSpec.Parse("localabstract:scrcpy2"), forwards[1].RemoteSpec);
            Assert.Equal("(reverse)", forwards[1].SerialNumber);
            Assert.Equal(ForwardSpec.Parse("tcp:100"), forwards[2].LocalSpec);
            Assert.Equal(ForwardSpec.Parse("localabstract:scrcpy3"), forwards[2].RemoteSpec);
            Assert.Equal("(reverse)", forwards[2].SerialNumber);

            protocolMock.Verify();
        }
    }
}
