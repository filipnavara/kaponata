// <copyright file="AdbShellClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> class shell methods.
    /// </summary>
    public class AdbShellClientTests
    {
        /// <summary>
        /// The <see cref="AdbClient.ExecuteRemoteShellCommandAsync(DeviceData, string, System.Threading.CancellationToken)"/> method throws on <c>ADB</c> server error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ExecuteRemoteShellCommand_ThrowsOnError_Async()
        {
            var stream = new MemoryStream(new byte[] { 4, 5, 6 });
            var adbProtocolMock = new Mock<AdbProtocol>(stream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            adbProtocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default)).Returns(Task.CompletedTask)
                .Verifiable();

            adbProtocolMock
                .Setup(p => p.WriteAsync("shell:testcommand", default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            adbProtocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(new AdbResponse(AdbResponseStatus.FAIL, "aiai"))
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default, false)).ReturnsAsync(adbProtocolMock.Object);
            var client = clientMock.Object;

            var exception = await Assert.ThrowsAsync<AdbException>(async () => await client.ExecuteRemoteShellCommandAsync(new DeviceData() { Serial = "123" }, "testcommand", default).ConfigureAwait(false));
            Assert.Equal("aiai", exception.Message);

            adbProtocolMock.Verify();
            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbClient.ExecuteRemoteShellCommandAsync(DeviceData, string, System.Threading.CancellationToken)"/> methods executes the given shell method on the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ExecuteRemoteShellCommand_ExecutesCommand_Async()
        {
            var stream = new MemoryStream(new byte[] { 4, 5, 6 });
            var adbProtocolMock = new Mock<AdbProtocol>(stream, true, NullLogger<AdbProtocol>.Instance)
            {
                CallBase = true,
            };

            adbProtocolMock
                .Setup(p => p.SetDeviceAsync(It.Is<DeviceData>(d => d.Serial == "123"), default)).Returns(Task.CompletedTask)
                .Verifiable();

            adbProtocolMock
                .Setup(p => p.WriteAsync("shell:testcommand", default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            adbProtocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(AdbResponse.Success)
                .Verifiable();

            var clientMock = new Mock<AdbClient>(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance)
            {
                CallBase = true,
            };

            clientMock.Setup(c => c.TryConnectToAdbAsync(default, false)).ReturnsAsync(adbProtocolMock.Object);
            var client = clientMock.Object;

            var shellStream = await client.ExecuteRemoteShellCommandAsync(new DeviceData() { Serial = "123" }, "testcommand", default).ConfigureAwait(false);

            Assert.Equal(4, shellStream.ReadByte());
            Assert.Equal(5, shellStream.ReadByte());
            Assert.Equal(6, shellStream.ReadByte());

            adbProtocolMock.Verify();
            clientMock.Verify();
        }
    }
}
