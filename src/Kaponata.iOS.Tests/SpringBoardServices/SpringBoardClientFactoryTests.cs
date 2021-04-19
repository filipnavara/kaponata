// <copyright file="SpringBoardClientFactoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.SpingBoardServices;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.SpringBoardServices
{
    /// <summary>
    /// Tests the <see cref="SpringBoardClientFactory"/> class.
    /// </summary>
    public class SpringBoardClientFactoryTests
    {
        /// <summary>
        /// The <see cref="SpringBoardClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(null, new DeviceContext(), Mock.Of<LockdownClientFactory>(), NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), null, Mock.Of<LockdownClientFactory>(), NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), null, NullLogger<SpringBoardClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClientFactory(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<LockdownClientFactory>(), null));
        }

        /// <summary>
        /// The <see cref="SpringBoardClientFactory.CreateAsync(CancellationToken)"/> method returns a properly
        /// configured <see cref="SpringBoardClient"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_Works_Async()
        {
            var lockdownClientFactory = new Mock<LockdownClientFactory>(MockBehavior.Strict);
            var muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            var context = new DeviceContext() { Device = new MuxerDevice() };

            var lockdownClient = new Mock<LockdownClient>(MockBehavior.Strict);
            lockdownClientFactory
                .Setup(l => l.CreateAsync(default))
                .ReturnsAsync(lockdownClient.Object)
                .Verifiable();

            lockdownClient
                .Setup(l => l.StartServiceAsync(SpringBoardClient.ServiceName, default))
                .ReturnsAsync(new ServiceDescriptor() { Port = 1234 })
                .Verifiable();

            muxerClient
                .Setup(m => m.ConnectAsync(context.Device, 1234, default))
                .ReturnsAsync(Stream.Null)
                .Verifiable();

            var factory = new SpringBoardClientFactory(muxerClient.Object, context, lockdownClientFactory.Object, NullLogger<SpringBoardClient>.Instance);

            await using (var client = await factory.CreateAsync(default).ConfigureAwait(false))
            {
            }

            lockdownClientFactory.Verify();
            lockdownClient.Verify();
            muxerClient.Verify();
        }
    }
}
