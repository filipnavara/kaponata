// <copyright file="PairingWorkerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.NotificationProxy;
using Kaponata.iOS.Workers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Workers
{
    /// <summary>
    /// Tests the <see cref="PairingWorker"/> class.
    /// </summary>
    public class PairingWorkerTests
    {
        private readonly Mock<MuxerClient> muxer;
        private readonly IServiceProvider provider;
        private readonly Mock<LockdownClient> lockdown;
        private readonly Mock<NotificationProxyClient> notificationProxyClient;
        private readonly MuxerDevice device;
        private readonly Mock<PairingRecordGenerator> pairingRecordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingWorkerTests"/> class.
        /// </summary>
        public PairingWorkerTests()
        {
            this.muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            this.notificationProxyClient = new Mock<NotificationProxyClient>(MockBehavior.Strict);
            this.lockdown = new Mock<LockdownClient>(MockBehavior.Strict);
            this.device = new MuxerDevice() { Udid = "my-udid" };
            this.pairingRecordGenerator = new Mock<PairingRecordGenerator>(MockBehavior.Strict);

            var lockdownFactory = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            lockdownFactory.Setup(f => f.CreateAsync(default)).ReturnsAsync(this.lockdown.Object);

            var notificationProxyFactory = new Mock<ClientFactory<NotificationProxyClient>>(MockBehavior.Strict);
            notificationProxyFactory.Setup(p => p.CreateAsync(NotificationProxyClient.InsecureServiceName, default)).ReturnsAsync(this.notificationProxyClient.Object);

            this.provider = new ServiceCollection()
                .AddSingleton(this.muxer.Object)
                .AddScoped((sp) => lockdownFactory.Object)
                .AddScoped((sp) => notificationProxyFactory.Object)
                .AddScoped<DeviceContext>((sp) => new DeviceContext() { Device = this.device })
                .AddScoped<PairingWorker>()
                .AddSingleton(this.pairingRecordGenerator.Object)
                .BuildServiceProvider();
        }

        /// <summary>
        /// The <see cref="PairingWorker"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(null, new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), null, Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), null, Mock.Of<ClientFactory<NotificationProxyClient>>(), Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), null, Mock.Of<PairingRecordGenerator>()));
            Assert.Throws<ArgumentNullException>(() => new PairingWorker(Mock.Of<MuxerClient>(), new DeviceContext(), Mock.Of<ClientFactory<LockdownClient>>(), Mock.Of<ClientFactory<NotificationProxyClient>>(), null));
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> returns the pairing record stored in the muxer if that pairing record
        /// is a valid pairing record.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_IsPaired_Works_Async()
        {
            this.notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            var pairingRecord = new PairingRecord();
            this.muxer.Setup(m => m.ReadPairingRecordAsync(this.device.Udid, default)).ReturnsAsync(pairingRecord);

            var pairingResult = new PairingResult() { Status = PairingStatus.Success };
            this.lockdown.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(pairingResult);

            using (var scope = this.provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
            }
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_Pairs_Async()
        {
            this.notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            this.muxer.Setup(m => m.ReadPairingRecordAsync(this.device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            this.lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            this.lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            this.muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            this.pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            var pairingResult = new PairingResult() { Status = PairingStatus.Success };
            this.lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            this.muxer.Setup(m => m.SavePairingRecordAsync(this.device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = this.provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
            }

            this.lockdown.Verify();
            this.muxer.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and waits for the user to accept the pairing response if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_PairsAndWaitsForUserResponse_Async()
        {
            var escrowBag = new byte[] { 5, 6, 7, 8 };
            var pairingResult = new PairingResult() { Status = PairingStatus.PairingDialogResponsePending };

            this.notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            this.notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.EscrowBag = escrowBag;
                    pairingResult.Status = PairingStatus.Success;
                })
                .Verifiable();

            this.muxer.Setup(m => m.ReadPairingRecordAsync(this.device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            this.lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            this.lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            this.muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            this.pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            this.lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            this.muxer.Setup(m => m.SavePairingRecordAsync(this.device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = this.provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
                Assert.Equal(escrowBag, pairingRecord.EscrowBag);
            }

            this.lockdown.Verify();
            this.muxer.Verify();
            this.notificationProxyClient.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and aborts the operation if the user rejects the pairing request.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_NotPaired_PairsAndHandlesUserRejection_Async()
        {
            var pairingResult = new PairingResult() { Status = PairingStatus.PairingDialogResponsePending };

            this.notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            this.notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.Status = PairingStatus.UserDeniedPairing;
                })
                .Verifiable();

            this.muxer.Setup(m => m.ReadPairingRecordAsync(this.device.Udid, default)).ReturnsAsync((PairingRecord)null);

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            this.lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey);
            this.lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress);
            this.muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid);

            var pairingRecord = new PairingRecord();
            this.pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord);

            this.lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            this.muxer.Setup(m => m.SavePairingRecordAsync(this.device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();
            this.muxer.Setup(m => m.DeletePairingRecordAsync(this.device.Udid, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = this.provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);
                Assert.Null(result);
            }

            this.lockdown.Verify();
            this.muxer.Verify();
            this.notificationProxyClient.Verify();
        }

        /// <summary>
        /// <see cref="PairingWorker.PairAsync(CancellationToken)"/> creates a new pairing record and pairs with the device, if no pairing
        /// record is currently available, and waits for the user to accept the pairing response if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PairAsync_PairedButDeniedPairing_AttemptsToPair_Async()
        {
            var escrowBag = new byte[] { 5, 6, 7, 8 };
            var pairingResult = new PairingResult() { Status = PairingStatus.UserDeniedPairing };

            this.notificationProxyClient.Setup(s => s.ObserveNotificationAsync(Notifications.RequestPair, default)).Returns(Task.CompletedTask).Verifiable();

            this.notificationProxyClient
                .Setup(s => s.ReadRelayNotificationAsync(default))
                .ReturnsAsync(Notifications.RequestPair)
                .Callback(() =>
                {
                    pairingResult.EscrowBag = escrowBag;
                    pairingResult.Status = PairingStatus.Success;
                })
                .Verifiable();

            var pairingRecord = new PairingRecord();
            this.muxer.Setup(m => m.ReadPairingRecordAsync(this.device.Udid, default)).ReturnsAsync(pairingRecord).Verifiable();

            var publicKey = new byte[] { 1, 2, 3, 4 };
            var wifiAddress = "AA:BB:CC";
            var buid = "123456789abcdef";

            this.lockdown.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();
            this.lockdown
                .Setup(l => l.UnpairAsync(pairingRecord, default))
                .Callback(() => { pairingResult.Status = PairingStatus.PairingDialogResponsePending; })
                .ReturnsAsync(pairingResult)
                .Verifiable();
            this.lockdown.Setup(l => l.GetPublicKeyAsync(default)).ReturnsAsync(publicKey).Verifiable();
            this.lockdown.Setup(l => l.GetWifiAddressAsync(default)).ReturnsAsync(wifiAddress).Verifiable();
            this.muxer.Setup(m => m.ReadBuidAsync(default)).ReturnsAsync(buid).Verifiable();
            this.muxer.Setup(m => m.DeletePairingRecordAsync(this.device.Udid, default)).Returns(Task.CompletedTask).Verifiable();

            this.pairingRecordGenerator.Setup(p => p.Generate(publicKey, buid)).Returns(pairingRecord).Verifiable();

            this.lockdown.Setup(l => l.PairAsync(pairingRecord, default)).ReturnsAsync(pairingResult).Verifiable();

            this.muxer.Setup(m => m.SavePairingRecordAsync(this.device.Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            using (var scope = this.provider.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<PairingWorker>();

                var result = await worker.PairAsync(default).ConfigureAwait(false);

                Assert.Same(pairingRecord, result);
                Assert.Equal(escrowBag, pairingRecord.EscrowBag);
            }

            this.lockdown.Verify();
            this.muxer.Verify();
            this.notificationProxyClient.Verify();
        }
    }
}
