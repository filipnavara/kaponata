// <copyright file="LockdownClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownClient"/> class.
    /// </summary>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// The <see cref="LockdownClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new LockdownClient((Stream)null, Mock.Of<MuxerClient>(), new MuxerDevice()));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Stream.Null, null, new MuxerDevice()));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Stream.Null, Mock.Of<MuxerClient>(), null));

            Assert.Throws<ArgumentNullException>(() => new LockdownClient((LockdownProtocol)null, Mock.Of<MuxerClient>(), new MuxerDevice()));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Mock.Of<LockdownProtocol>(), null, new MuxerDevice()));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Mock.Of<LockdownProtocol>(), Mock.Of<MuxerClient>(), null));
        }

        /// <summary>
        /// The <see cref="LockdownClient.ConnectAsync(MuxerClient, MuxerDevice, CancellationToken)"/> method works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Connect_Works_Async()
        {
            // Sample traffic from https://www.theiphonewiki.com/wiki/Usbmux ("lockdownd protocol")
            var muxer = new Mock<MuxerClient>();
            var device = new MuxerDevice();

            using (var traceStream = new TraceStream("Lockdown/connect-device.bin", "Lockdown/connect-host.bin"))
            {
                muxer
                    .Setup(m => m.ConnectAsync(device, 0xF27E, default))
                    .ReturnsAsync(traceStream);

                await using (await LockdownClient.ConnectAsync(muxer.Object, device, default))
                {
                    // The trace stream will assert the correct data is exchanged.
                }
            }
        }
    }
}
