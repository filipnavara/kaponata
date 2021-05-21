// <copyright file="DeveloperDiskProvisionerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.MobileImageMounter;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Sidecars.Tests
{
    /// <summary>
    /// Tests the <see cref="DeveloperDiskProvisioner"/> class.
    /// </summary>
    public class DeveloperDiskProvisionerTests
    {
        /// <summary>
        /// The <see cref="DeveloperDiskProvisioner"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeveloperDiskProvisioner(null, Mock.Of<DeviceServiceProvider>(), NullLogger<DeveloperDiskProvisioner>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DeveloperDiskProvisioner(Mock.Of<DeveloperDiskStore>(), null, NullLogger<DeveloperDiskProvisioner>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DeveloperDiskProvisioner(Mock.Of<DeveloperDiskStore>(), Mock.Of<DeviceServiceProvider>(), null));
        }

        /// <summary>
        /// <see cref="DeveloperDiskProvisioner.ProvisionDeveloperDiskAsync(string, CancellationToken)"/> returns <see langword="true"/>
        /// if a developer disk is already mounted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionAsync_AlreadyMounted_DoesNothing_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var provider = new Mock<DeviceServiceProvider>(MockBehavior.Strict);
            const string udid = "udid";

            var scope = new Mock<DeviceServiceScope>(MockBehavior.Strict);
            provider.Setup(p => p.CreateDeviceScopeAsync(udid, default)).ReturnsAsync(scope.Object);

            var lockdown = new Mock<LockdownClient>(MockBehavior.Strict);
            var mounter = new Mock<MobileImageMounterClient>(MockBehavior.Strict);
            scope.Setup(s => s.StartServiceAsync<LockdownClient>(default)).ReturnsAsync(lockdown.Object);
            scope.Setup(s => s.StartServiceAsync<MobileImageMounterClient>(default)).ReturnsAsync(mounter.Object);

            var provisioner = new DeveloperDiskProvisioner(store.Object, provider.Object, NullLogger<DeveloperDiskProvisioner>.Instance);

            mounter
                .Setup(m => m.LookupImageAsync("Developer", default))
                .ReturnsAsync(
                new LookupImageResponse()
                {
                    ImageSignature = new List<byte[]>() { Array.Empty<byte>() },
                });

            Assert.True(await provisioner.ProvisionDeveloperDiskAsync(udid, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="DeveloperDiskProvisioner.ProvisionDeveloperDiskAsync(string, CancellationToken)"/> returns <see langword="false"/>
        /// if a developer disk is not mounted and no developer disk is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionAsync_NotMounted_NotAvailable_Fails_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var provider = new Mock<DeviceServiceProvider>(MockBehavior.Strict);
            const string udid = "udid";

            var scope = new Mock<DeviceServiceScope>(MockBehavior.Strict);
            provider.Setup(p => p.CreateDeviceScopeAsync(udid, default)).ReturnsAsync(scope.Object);

            var lockdown = new Mock<LockdownClient>(MockBehavior.Strict);
            var mounter = new Mock<MobileImageMounterClient>(MockBehavior.Strict);
            scope.Setup(s => s.StartServiceAsync<LockdownClient>(default)).ReturnsAsync(lockdown.Object);
            scope.Setup(s => s.StartServiceAsync<MobileImageMounterClient>(default)).ReturnsAsync(mounter.Object);

            var provisioner = new DeveloperDiskProvisioner(store.Object, provider.Object, NullLogger<DeveloperDiskProvisioner>.Instance);

            mounter
                .Setup(m => m.LookupImageAsync("Developer", default))
                .ReturnsAsync(
                new LookupImageResponse() { ImageSignature = new List<byte[]>() });

            lockdown.Setup(l => l.GetValueAsync("ProductVersion", default)).ReturnsAsync("14.1.1");

            store.Setup(s => s.GetAsync(new Version(14, 1), default)).ReturnsAsync((DeveloperDisk)null);
            store.Setup(s => s.GetAsync(new Version(14, 1, 1), default)).ReturnsAsync((DeveloperDisk)null);

            Assert.False(await provisioner.ProvisionDeveloperDiskAsync(udid, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="DeveloperDiskProvisioner.ProvisionDeveloperDiskAsync(string, CancellationToken)"/> returns <see langword="true"/>
        /// if a developer disk is not mounted, but a matching developer disk can be mounted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ProvisionAsync_NotMounted_Available_Success_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var provider = new Mock<DeviceServiceProvider>(MockBehavior.Strict);
            const string udid = "udid";

            var scope = new Mock<DeviceServiceScope>(MockBehavior.Strict);
            provider.Setup(p => p.CreateDeviceScopeAsync(udid, default)).ReturnsAsync(scope.Object);

            var lockdown = new Mock<LockdownClient>(MockBehavior.Strict);
            var mounter = new Mock<MobileImageMounterClient>(MockBehavior.Strict);
            scope.Setup(s => s.StartServiceAsync<LockdownClient>(default)).ReturnsAsync(lockdown.Object);
            scope.Setup(s => s.StartServiceAsync<MobileImageMounterClient>(default)).ReturnsAsync(mounter.Object);

            var disk = new DeveloperDisk()
            {
                Image = Stream.Null,
                Signature = Array.Empty<byte>(),
            };

            var provisioner = new DeveloperDiskProvisioner(store.Object, provider.Object, NullLogger<DeveloperDiskProvisioner>.Instance);

            mounter
                .Setup(m => m.LookupImageAsync("Developer", default))
                .ReturnsAsync(
                new LookupImageResponse() { ImageSignature = new List<byte[]>() });

            lockdown.Setup(l => l.GetValueAsync("ProductVersion", default)).ReturnsAsync("14.1.1");

            store.Setup(s => s.GetAsync(new Version(14, 1, 1), default)).ReturnsAsync((DeveloperDisk)null);
            store.Setup(s => s.GetAsync(new Version(14, 1), default)).ReturnsAsync(disk);

            mounter.Setup(s => s.UploadImageAsync(disk.Image, "Developer", disk.Signature, default)).Returns(Task.CompletedTask);
            mounter.Setup(s => s.MountImageAsync(disk.Signature, "Developer", default)).Returns(Task.CompletedTask);

            Assert.True(await provisioner.ProvisionDeveloperDiskAsync(udid, default).ConfigureAwait(false));
        }
    }
}
