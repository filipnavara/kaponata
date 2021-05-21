// <copyright file="PairingRecordProvisionerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS;
using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.Workers;
using Kaponata.Kubernetes.PairingRecords;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Sidecars.Tests
{
    /// <summary>
    /// Tests the <see cref="PairingRecordProvisioner"/> class.
    /// </summary>
    public class PairingRecordProvisionerTests
    {
        private const string Udid = "udid";

        private readonly MuxerDevice device;
        private readonly Mock<PairingWorker> pairingWorker;
        private readonly Mock<KubernetesPairingRecordStore> kubernetesPairingRecordStore;
        private readonly Mock<MuxerClient> muxerClient;
        private readonly Mock<LockdownClient> lockdownClient;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingRecordProvisionerTests"/> class.
        /// </summary>
        public PairingRecordProvisionerTests()
        {
            this.pairingWorker = new Mock<PairingWorker>(MockBehavior.Strict);
            this.kubernetesPairingRecordStore = new Mock<KubernetesPairingRecordStore>(MockBehavior.Strict);
            this.muxerClient = new Mock<MuxerClient>(MockBehavior.Strict);
            this.lockdownClient = new Mock<LockdownClient>(MockBehavior.Strict);

            this.device = new MuxerDevice() { Udid = Udid };
            this.muxerClient.Setup(m => m.ListDevicesAsync(default)).ReturnsAsync(new Collection<MuxerDevice>() { this.device });

            var lockdownFactoryMock = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            lockdownFactoryMock.Setup(m => m.CreateAsync(default)).ReturnsAsync(this.lockdownClient.Object);

            this.serviceProvider =
                new ServiceCollection()
                .AddLogging()
                .AddSingleton(this.pairingWorker.Object)
                .AddSingleton(this.kubernetesPairingRecordStore.Object)
                .AddSingleton(this.muxerClient.Object)
                .AddSingleton(this.lockdownClient.Object)
                .AddSingleton(lockdownFactoryMock.Object)
                .AddSingleton<PairingRecordProvisioner>()
                .AddSingleton<DeviceContext>()
                .AddSingleton<DeviceServiceProvider>()
                .BuildServiceProvider();
        }

        /// <summary>
        /// The <see cref="PairingRecordProvisioner"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("muxerClient", () => new PairingRecordProvisioner(null, Mock.Of<KubernetesPairingRecordStore>(), Mock.Of<DeviceServiceProvider>(), NullLogger<PairingRecordProvisioner>.Instance));
            Assert.Throws<ArgumentNullException>("kubernetesPairingRecordStore", () => new PairingRecordProvisioner(Mock.Of<MuxerClient>(), null, Mock.Of<DeviceServiceProvider>(), NullLogger<PairingRecordProvisioner>.Instance));
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new PairingRecordProvisioner(Mock.Of<MuxerClient>(), Mock.Of<KubernetesPairingRecordStore>(), null, NullLogger<PairingRecordProvisioner>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new PairingRecordProvisioner(Mock.Of<MuxerClient>(), Mock.Of<KubernetesPairingRecordStore>(), Mock.Of<DeviceServiceProvider>(), null));
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> creates a new pairing record
        /// if no pairing record is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_NoPairingRecord_StartsPairing_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var pairingRecord = new PairingRecord();

            // A pairing record exists in the muxer nor in the pairing record store
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync((PairingRecord)null).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync((PairingRecord)null).Verifiable();

            // A new pairing record is generated
            this.pairingWorker.Setup(w => w.PairAsync(default)).ReturnsAsync(pairingRecord).Verifiable();

            // A null value is returned to the caller, as the pairing operation is still in progress.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Null(result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> uses the existing pairing record
        /// if one is available at the muxer level, and pushes that pairing record to the cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_MuxerPairingRecord_UpdatesCluster_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var pairingRecord = new PairingRecord();

            // A pairing record exists in the muxer but not at the cluster level
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync(pairingRecord).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync((PairingRecord)null).Verifiable();

            this.lockdownClient.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(true).Verifiable();

            // This pairing record is stored at the cluster level
            this.kubernetesPairingRecordStore.Setup(m => m.WriteAsync(Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            // The pairing record which was available at the muxer level is returned to the caller.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Equal(pairingRecord, result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> uses the cluster pairing
        /// record and pushes that to the muxer if one is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_ClusterPairingRecord_UpdatesMuxer_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var pairingRecord = new PairingRecord();

            // A pairing record exists in the muxer but not at the cluster level
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync((PairingRecord)null).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync(pairingRecord).Verifiable();

            this.lockdownClient.Setup(l => l.ValidatePairAsync(pairingRecord, default)).ReturnsAsync(true).Verifiable();

            // This pairing record is stored at the cluster level
            this.muxerClient.Setup(m => m.SavePairingRecordAsync(Udid, pairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            // The cluster pairing record is returned to the caller.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Equal(pairingRecord, result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> returns a pairing record
        /// if the same is available at the muxer and cluster level.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_MuxerAndClusterHavePairingRecord_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var clusterPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId" };
            var muxerPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId" };

            // A pairing record exists in the muxer and at the cluster level
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync(muxerPairingRecord).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync(clusterPairingRecord).Verifiable();

            this.lockdownClient.Setup(l => l.ValidatePairAsync(muxerPairingRecord, default)).ReturnsAsync(true).Verifiable();

            // The correct pairing record is returned to the caller.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Equal(clusterPairingRecord, result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> updates the local pairing
        /// record if that pairing record is outdated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_LocalPairingRecordOutdated_UsesClusterValue_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var clusterPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId1" };
            var muxerPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId2" };

            // A pairing record exists in the muxer and at the cluster level
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync(muxerPairingRecord).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync(clusterPairingRecord).Verifiable();

            this.lockdownClient.Setup(l => l.ValidatePairAsync(muxerPairingRecord, default)).ReturnsAsync(false).Verifiable();
            this.lockdownClient.Setup(l => l.ValidatePairAsync(clusterPairingRecord, default)).ReturnsAsync(true).Verifiable();

            // The muxer pairing record is updated
            this.muxerClient.Setup(m => m.DeletePairingRecordAsync(Udid, default)).Returns(Task.CompletedTask).Verifiable();
            this.muxerClient.Setup(m => m.SavePairingRecordAsync(Udid, clusterPairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            // The correct pairing record is returned to the caller.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Equal(clusterPairingRecord, result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }

        /// <summary>
        /// <see cref="PairingRecordProvisioner.ProvisionPairingRecordAsync(string, CancellationToken)"/> updates the local pairing
        /// record if that pairing record is outdated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionPairingRecordAsync_ClusterPairingRecordOutdated_UsesLocalValue_Async()
        {
            var provisioner = this.serviceProvider.GetRequiredService<PairingRecordProvisioner>();

            var clusterPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId1" };
            var muxerPairingRecord = new PairingRecord() { SystemBUID = "Buid", HostId = "HostId2" };

            // A pairing record exists in the muxer and at the cluster level
            this.muxerClient.Setup(m => m.ReadPairingRecordAsync(Udid, default)).ReturnsAsync(muxerPairingRecord).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.ReadAsync(Udid, default)).ReturnsAsync(clusterPairingRecord).Verifiable();

            this.lockdownClient.Setup(l => l.ValidatePairAsync(muxerPairingRecord, default)).ReturnsAsync(true).Verifiable();

            // The cluster-level pairing record is updated
            this.kubernetesPairingRecordStore.Setup(m => m.DeleteAsync(Udid, default)).Returns(Task.CompletedTask).Verifiable();
            this.kubernetesPairingRecordStore.Setup(m => m.WriteAsync(Udid, muxerPairingRecord, default)).Returns(Task.CompletedTask).Verifiable();

            // The correct pairing record is returned to the caller.
            var result = await provisioner.ProvisionPairingRecordAsync(Udid, default);
            Assert.Equal(muxerPairingRecord, result);

            this.muxerClient.Verify();
            this.kubernetesPairingRecordStore.Verify();
            this.pairingWorker.Verify();
        }
    }
}
