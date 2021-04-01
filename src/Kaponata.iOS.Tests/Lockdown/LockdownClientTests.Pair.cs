﻿// <copyright file="LockdownClientTests.Pair.cs" company="Quamotion bv">
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
    /// Tests the Pair methods on the <see cref="LockdownClient"/> class.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ValidatesArguments_Async()
        {
            await using (var lockdown = new LockdownClient(Mock.Of<LockdownProtocol>(), Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("pairingRecord", () => lockdown.PairAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.NotNull(request.PairingOptions);
                    Assert.True(request.PairingOptions.ExtendedPairingErrors);
                    Assert.Equal("Pair", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairResult.Success, result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> parses well-known
        /// error messages.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ParsesErrors_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("Error", "PairingDialogResponsePending");

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairResult.PairingDialogResponsePending, result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> throws on unknown errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ThrowsOnError_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();
            dict.Add("Error", "Unknown");

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                await Assert.ThrowsAsync<LockdownException>(() => lockdown.PairAsync(pairingRecord, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.PairAsync(PairingRecord, CancellationToken)"/> returns <see langword="null"/> when
        /// the device disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_ReturnsNullOnDisconnect_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Returns(Task.CompletedTask);

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync((NSDictionary)null);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var result = await lockdown.PairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Null(result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.UnpairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UnpairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.Null(request.PairingOptions);
                    Assert.Equal("Unpair", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var result = await lockdown.UnpairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairResult.Success, result);
            }
        }

        /// <summary>
        /// <see cref="LockdownClient.ValidatePairAsync(PairingRecord, CancellationToken)"/> works.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidatePairAsync_Works_Async()
        {
            var pairingRecord = new PairingRecord();

            var protocol = new Mock<LockdownProtocol>();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, ct) =>
                {
                    var request = Assert.IsType<PairRequest>(message);

                    Assert.Same(pairingRecord, request.PairRecord);
                    Assert.Null(request.PairingOptions);
                    Assert.Equal("ValidatePair", request.Request);
                })
                .Returns(Task.CompletedTask);

            var dict = new NSDictionary();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict);

            await using (var lockdown = new LockdownClient(protocol.Object, Mock.Of<MuxerClient>(), new MuxerDevice()))
            {
                var result = await lockdown.ValidatePairAsync(pairingRecord, default).ConfigureAwait(false);
                Assert.Equal(PairResult.Success, result);
            }
        }
    }
}
