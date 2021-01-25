// <copyright file="AdbProtocolTests.cs" company="Quamotion bv">
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
    /// Tests the <see cref="AdbProtocol"/> class.
    /// </summary>
    public class AdbProtocolTests
    {
        /// <summary>
        /// The <see cref="AdbProtocol"/> class disposes of the underlying stream if it owns the stream.
        /// </summary>
        /// <param name="ownsStream">
        /// A value indicating whether the <see cref="AdbProtocol"/> owns the stream.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposeAsync_DisposesStreamIfNeeded_Async(bool ownsStream)
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);

            if (ownsStream)
            {
                stream.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            }

            var protocol = new AdbProtocol(stream.Object, ownsStream, NullLogger<AdbProtocol>.Instance);
            await protocol.DisposeAsync();

            stream.Verify();
        }

        /// <summary>
        /// The <see cref="AdbProtocol"/> constructor checks for <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("stream", () => new AdbProtocol(null, ownsStream: true, NullLogger<AdbProtocol>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new AdbProtocol(Stream.Null, ownsStream: true, null));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteAsync(string, System.Threading.CancellationToken)"/> sends the given data to the <c>ADB</c> server.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <param name="output">
        /// The serialized output string.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("Test", "0004Test")]
        [InlineData("host:version", "000Chost:version")]
        [InlineData("host:kill", "0009host:kill")]
        [InlineData("tcp:1981:127.0.0.1", "0012tcp:1981:127.0.0.1")]
        public async Task WriteAsync_SendData_Async(string input, string output)
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await protocol.WriteAsync(input, default).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            Assert.Equal(output, await reader.ReadToEndAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteAsync(string, System.Threading.CancellationToken)"/> sends the given data to the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteAsync_SendDataStream_Async()
        {
            await using MemoryStream inputStream = new MemoryStream();
            using var writer = new StreamWriter(inputStream);
            await writer.WriteAsync("Tester").ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            inputStream.Position = 0;

            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await protocol.WriteAsync(inputStream, default).ConfigureAwait(false);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            Assert.Equal("Tester", await reader.ReadToEndAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteAsync(string, System.Threading.CancellationToken)"/> throws an exception when the message is invalid.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteAsync_ThrowsOnInvalidMessage_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.WriteAsync(string.Empty, default).ConfigureAwait(false));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.WriteAsync((string)null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.WriteAsync(Stream, CancellationToken)"/> throws an exception when the message is invalid.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteAsync_ThrowsOnInvalidStream_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.WriteAsync((Stream)null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadStringAsync(int, System.Threading.CancellationToken)"/> method receives a <see cref="string"/> from the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadString_ReceivesString_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync("Tester").ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            var message = await protocol.ReadStringAsync(6, default).ConfigureAwait(false);

            Assert.Equal("Tester", message);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadStringAsync(int, System.Threading.CancellationToken)"/> method returns null when server provides insufficient data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadString_NullWhenInsufficientData_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync("Tester").ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            Assert.Null(await protocol.ReadStringAsync(10, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadUInt16Async(CancellationToken)"/> method receives an <see cref="int"/> from the <c>ADB</c> server.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("0004", 4)]
        [InlineData("000C", 12)]
        [InlineData("0009", 9)]
        [InlineData("0012", 18)]
        public async Task ReadInt16_ReceivesInt_Async(string input, int value)
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(input).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            var message = await protocol.ReadUInt16Async(default).ConfigureAwait(false);

            Assert.Equal(value, message);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadUInt16Async(CancellationToken)"/> throws an exception when the int cannot be read.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadInt16_ThrowsOnStreamError_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync("000").ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await protocol.ReadUInt16Async(default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadAdbResponseStatusAsync(CancellationToken)"/> method returns the right status.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("OKAY", AdbResponseStatus.OKAY)]
        [InlineData("FAIL", AdbResponseStatus.FAIL)]
        public async Task ReadAdbResponseStatus_ReturnsStatus_Async(string input, AdbResponseStatus status)
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(input).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            var value = await protocol.ReadAdbResponseStatusAsync(default).ConfigureAwait(false);

            Assert.Equal(status, value);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadAdbResponseAsync(System.Threading.CancellationToken)"/> method receives a <see cref="AdbResponse"/> from the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadAdbResponse_ReceivesAdbResponse_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.ReadAdbResponseStatusAsync(CancellationToken.None)).ReturnsAsync(AdbResponseStatus.OKAY);
            protocolMock.Setup(p => p.ReadStringAsync(4, CancellationToken.None)).ReturnsAsync("OKAY");

            var protocol = protocolMock.Object;

            var response = await protocol.ReadAdbResponseAsync(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(string.Empty, response.Message);
            Assert.Equal(AdbResponseStatus.OKAY, response.Status);
        }

        /// <summary>
        /// Tests the <see cref="AdbProtocol.ReadAdbResponseAsync(System.Threading.CancellationToken)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadAdbResponseTest_Fail_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.ReadUInt16Async(CancellationToken.None)).ReturnsAsync((ushort)8);
            protocolMock.Setup(p => p.ReadAdbResponseStatusAsync(CancellationToken.None)).ReturnsAsync(AdbResponseStatus.FAIL);
            protocolMock.Setup(p => p.ReadStringAsync(8, CancellationToken.None)).ReturnsAsync("UnitTest");

            var protocol = protocolMock.Object;

            var response = await protocol.ReadAdbResponseAsync(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("UnitTest", response.Message);
            Assert.Equal(AdbResponseStatus.FAIL, response.Status);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.EnsureValidAdbResponse(AdbResponse)"/> throws an exception when an <see cref="AdbResponseStatus.FAIL"/> is responsed.
        /// </summary>
        [Fact]
        public void EnsureValidAdbResponse_ThrowsException()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var protocol = protocolMock.Object;
            var exception = Assert.Throws<InvalidDataException>(() => protocol.EnsureValidAdbResponse(new AdbResponse()
            {
                Status = AdbResponseStatus.FAIL,
                Message = "Aiai",
            }));

            Assert.Equal("Aiai", exception.Message);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.EnsureValidAdbResponse(AdbResponse)"/> throws no exception when an <see cref="AdbResponseStatus.OKAY"/> is responsed.
        /// </summary>
        [Fact]
        public void EnsureValidAdbResponse_PassWhenValid()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var protocol = protocolMock.Object;
            protocol.EnsureValidAdbResponse(AdbResponse.Success);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.SetDeviceAsync(DeviceData, CancellationToken)"/> sends the set device command to the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SetDeviceAsync_SendsDeviceCommand_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.WriteAsync("host:transport:123", default))
                    .Verifiable();
            protocolMock
                    .Setup(p => p.ReadAdbResponseAsync(default))
                    .ReturnsAsync(AdbResponse.Success);

            var protocol = protocolMock.Object;
            await protocol.SetDeviceAsync(
                new DeviceData() { Serial = "123" },
                default).ConfigureAwait(false);

            protocolMock.Verify();
        }

        /// <summary>
        /// The <see cref="AdbProtocol.SetDeviceAsync(DeviceData, CancellationToken)"/> throws exception when an invalid serial is provided.
        /// </summary>
        /// <param name="serial">
        /// An invalid serial.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SetDeviceAsync_ThrowsOnInvalidSerial_Async(string serial)
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<ArgumentException>(async () => await protocol.SetDeviceAsync(new DeviceData() { Serial = serial }, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.ReadStringAsync(CancellationToken)"/> reads out a string from the <c>ADB</c> server.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadString_ReadString_Async()
        {
            await using MemoryStream stream = new MemoryStream();
            await using var protocol = new AdbProtocol(stream, ownsStream: true, NullLogger<AdbProtocol>.Instance);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync("Tester").ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            var message = await protocol.ReadStringAsync(default).ConfigureAwait(false);

            Assert.Equal("Tester", message);
        }

        /// <summary>
        /// The <see cref="AdbProtocol.SetDeviceAsync(DeviceData, CancellationToken)"/> throws exception when invalid device data is provided.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SetDeviceAsync_ThrowsOnInvalidDeviceData_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            var protocol = protocolMock.Object;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await protocol.SetDeviceAsync(null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// The <see cref="AdbProtocol.SetDeviceAsync(DeviceData, CancellationToken)"/> throws exception the <c>ADB</c> server responds with FAIL.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SetDeviceAsync_ThrowsWhenSetDeviceFails_Async()
        {
            var protocolMock = new Mock<AdbProtocol>()
            {
                CallBase = true,
            };

            protocolMock.Setup(p => p.WriteAsync("host:transport:123", default))
                    .Verifiable();
            protocolMock
                .Setup(p => p.ReadAdbResponseAsync(default))
                .ReturnsAsync(new AdbResponse()
                {
                    Status = AdbResponseStatus.FAIL,
                    Message = "Aiai",
                })
                .Verifiable();

            var protocol = protocolMock.Object;
            var exception = await Assert.ThrowsAsync<InvalidDataException>(async () => await protocol.SetDeviceAsync(new DeviceData() { Serial = "123" }, default).ConfigureAwait(false));
            Assert.Equal("Aiai", exception.Message);
            protocolMock.Verify();
        }
    }
}
