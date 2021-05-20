// <copyright file="MobileImageMounterClientFactoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.MobileImageMounter;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.PropertyLists;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="MobileImageMounterClient"/> class.
    /// </summary>
    public class MobileImageMounterClientFactoryTests
    {
        /// <summary>
        /// The <see cref="MobileImageMounterClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClientFactory(null, new DeviceContext(), Mock.Of<PropertyListProtocolFactory>(), Mock.Of<LockdownClientFactory>(), NullLogger<MobileImageMounterClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClientFactory(Mock.Of<MuxerClient>(), null, Mock.Of<PropertyListProtocolFactory>(), Mock.Of<LockdownClientFactory>(), NullLogger<MobileImageMounterClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<LockdownClientFactory>(), NullLogger<MobileImageMounterClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<PropertyListProtocolFactory>(), null, NullLogger<MobileImageMounterClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<PropertyListProtocolFactory>(), Mock.Of<LockdownClientFactory>(), null));
        }

        /// <summary>
        /// The <see cref="MobileImageMounterClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="MobileImageMounterClient"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();
            var sessionResponse = new StartSessionResponse() { SessionID = "1234" };
            var lockdownClientFactory = new Mock<LockdownClientFactory>(MockBehavior.Strict);
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            var context = new DeviceContext() { Device = new MuxerDevice(), PairingRecord = pairingRecord };

            var lockdownClient = new Mock<LockdownClient>(MockBehavior.Strict);
            lockdownClientFactory
                .Setup(l => l.CreateAsync(default))
                .ReturnsAsync(lockdownClient.Object)
                .Verifiable();

            lockdownClient
                .Setup(l => l.StartSessionAsync(pairingRecord, default))
                .ReturnsAsync(sessionResponse);

            lockdownClient
                .Setup(l => l.StartServiceAsync(MobileImageMounterClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234 })
                .Verifiable();

            lockdownClient
                .Setup(l => l.StopSessionAsync(sessionResponse.SessionID, default))
                .Returns(Task.CompletedTask);

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var factory = new MobileImageMounterClientFactory(muxerClient.Object, context, new PropertyListProtocolFactory(), lockdownClientFactory.Object, NullLogger<MobileImageMounterClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
        }

        /// <summary>
        /// The <see cref="MobileImageMounterClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="MobileImageMounterClient"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_WithSessionSSL_Works_Async()
        {
            var pairingRecord = new PairingRecord();
            var sessionResponse = new StartSessionResponse() { SessionID = "1234" };
            var lockdownClientFactory = new Mock<LockdownClientFactory>(MockBehavior.Strict);
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            var context = new DeviceContext() { Device = new MuxerDevice(), PairingRecord = pairingRecord };

            var lockdownClient = new Mock<LockdownClient>(MockBehavior.Strict);
            lockdownClientFactory
                .Setup(l => l.CreateAsync(default))
                .ReturnsAsync(lockdownClient.Object)
                .Verifiable();

            lockdownClient
                .Setup(l => l.StartSessionAsync(pairingRecord, default))
                .ReturnsAsync(sessionResponse);

            lockdownClient
                .Setup(l => l.StartServiceAsync(MobileImageMounterClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234, EnableServiceSSL = true })
                .Verifiable();

            lockdownClient
                .Setup(l => l.StopSessionAsync(sessionResponse.SessionID, default))
                .Returns(Task.CompletedTask);

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var protocolMock = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocolMock.Setup(p => p.EnableSslAsync(pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();
            protocolMock.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var protocolFactory = new Mock<PropertyListProtocolFactory>(MockBehavior.Strict);
            protocolFactory.Setup(p => p.Create(Stream.Null, true, NullLogger<MobileImageMounterClient>.Instance)).Returns(protocolMock.Object);

            var factory = new MobileImageMounterClientFactory(muxerClient.Object, context, protocolFactory.Object, lockdownClientFactory.Object, NullLogger<MobileImageMounterClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
            protocolFactory.Verify();
            protocolMock.Verify();
        }
    }
}
