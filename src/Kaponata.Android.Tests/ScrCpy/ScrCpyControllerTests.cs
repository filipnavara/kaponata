// <copyright file="ScrCpyControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.ScrCpy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="ScrCpyController"/> class.
    /// </summary>
    public class ScrCpyControllerTests
    {
        /// <summary>
        /// The <see cref="ScrCpyController"/> constructor validate the argments being passed.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            using var stream = new MemoryStream();
            Assert.Throws<ArgumentNullException>(() => new ScrCpyController(new DeviceInfo() { }, stream, null));
            Assert.Throws<ArgumentNullException>(() => new ScrCpyController(new DeviceInfo() { }, null, NullLogger<ScrCpyController>.Instance));
        }

        /// <summary>
        /// The <see cref="ScrCpyClient"/> constructor initializes the properties.
        /// </summary>
        [Fact]
        public void Constructor_Properties()
        {
            using var stream = new MemoryStream();
            var logger = new NullLogger<ScrCpyController>();
            var controller = new ScrCpyController(new DeviceInfo() { DeviceName = "123" }, stream, logger);

            Assert.Equal("123", controller.DeviceInfo.DeviceName);
        }

        /// <summary>
        /// The <see cref="ScrCpyController.SendControlMessageAsync(IControlMessage, System.Threading.CancellationToken)"/> throws when invalid device info is passed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SendControlMessage_ValidatesArgument_Async()
        {
            using var stream = new MemoryStream();
            var controller = new ScrCpyController(new DeviceInfo() { DeviceName = "123" }, stream, NullLogger<ScrCpyController>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await controller.SendControlMessageAsync(null, default));
        }

        /// <summary>
        /// The <see cref="ScrCpyController.SendControlMessageAsync(IControlMessage, System.Threading.CancellationToken)"/> sends the message.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SendControlMessage_SendsMessage_Async()
        {
            using var stream = new MemoryStream();
            var controller = new ScrCpyController(new DeviceInfo() { DeviceName = "123" }, stream, NullLogger<ScrCpyController>.Instance);

            var messageMock = new Mock<IControlMessage>();
            messageMock.SetupGet(m => m.BinarySize).Returns(100);
            messageMock.Setup(m => m.Write(It.IsAny<Memory<byte>>())).Callback<Memory<byte>>(s => new byte[] { 1, 2, 3 }.CopyTo(s));

            await controller.SendControlMessageAsync(messageMock.Object, default).ConfigureAwait(false);

            stream.Position = 0;
            Assert.Equal(100, stream.Length);
            Assert.Equal(1, stream.ReadByte());
            Assert.Equal(2, stream.ReadByte());
            Assert.Equal(3, stream.ReadByte());
        }
    }
}
