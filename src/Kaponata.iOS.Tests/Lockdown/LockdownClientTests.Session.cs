// <copyright file="LockdownClientTests.Session.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <content>
    /// Tests the Session-related methods of the <see cref="LockdownClient"/> class.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_ValidatesArguments_Async()
        {
            var client = new LockdownClient(Mock.Of<LockdownProtocol>(), Mock.Of<MuxerClient>(), new MuxerDevice());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.StartSessionAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_Works_Async()
        {
            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var startSessionRequest = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("buid", startSessionRequest.SystemBUID);
                    Assert.Equal("id", startSessionRequest.HostID);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("SessionID", "123");
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var client = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var response = await client.StartSessionAsync(
                    new PairingRecord()
                    {
                        SystemBUID = "buid",
                        HostId = "id",
                    },
                    default).ConfigureAwait(false);

                Assert.Equal("123", response.SessionID);
                Assert.False(response.EnableSessionSSL);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StartSessionAsync(PairingRecord, CancellationToken)"/> throws an error when
        /// the device returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartSessionAsync_ThrowsOnError_Async()
        {
            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var startSessionRequest = Assert.IsType<StartSessionRequest>(message);

                    Assert.Equal("buid", startSessionRequest.SystemBUID);
                    Assert.Equal("id", startSessionRequest.HostID);
                })
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            response.Add("Error", "error");
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            await using (var client = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                await Assert.ThrowsAsync<LockdownException>(
                    () => client.StartSessionAsync(
                    new PairingRecord()
                    {
                        SystemBUID = "buid",
                        HostId = "id",
                    },
                    default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.StopSessionAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StopSessionAsync_ValidatesArguments_Async()
        {
            var client = new LockdownClient(Mock.Of<LockdownProtocol>(), Mock.Of<MuxerClient>(), new MuxerDevice());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.StopSessionAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="LockdownClient.StopSessionAsync(string, CancellationToken)"/> throws when the device
        /// returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StopSessionAsync_ThrowsOnError_Async()
        {
            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var stopSessionRequest = Assert.IsType<StopSessionRequest>(message);

                    Assert.Equal("1234", stopSessionRequest.SessionID);
                })
                .Returns(Task.CompletedTask);

            var response = new NSDictionary();
            response.Add("Error", "error");
            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(response);

            await using (var client = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                await Assert.ThrowsAsync<LockdownException>(
                    () => client.StopSessionAsync("1234", default)).ConfigureAwait(false);
            }
        }
    }
}
