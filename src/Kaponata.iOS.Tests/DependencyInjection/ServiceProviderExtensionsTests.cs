// <copyright file="ServiceProviderExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="ServiceProviderExtensions"/> class.
    /// </summary>
    public class ServiceProviderExtensionsTests
    {
        /// <summary>
        /// <see cref="ServiceProviderExtensions.CreateDeviceScopeAsync(IServiceProvider, string, CancellationToken)"/>
        /// throws a <see cref="MuxerException"/> when no UDID is specified and a no devices are connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_NoDevice_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>() { });

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .BuildServiceProvider();

            await Assert.ThrowsAsync<MuxerException>(() => provider.CreateDeviceScopeAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ServiceProviderExtensions.CreateDeviceScopeAsync(IServiceProvider, string, CancellationToken)"/>
        /// throws a <see cref="MuxerException"/> when no UDID is specified and more than one device is connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_TwoDevices_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice(),
                    new MuxerDevice(),
                });

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            await Assert.ThrowsAsync<MuxerException>(() => provider.CreateDeviceScopeAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ServiceProviderExtensions.CreateDeviceScopeAsync(IServiceProvider, string, CancellationToken)"/>
        /// returns the connected device when no UDID is specified and only a single device is connected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_NoUdid_SingleDevice_WorksAsync()
        {
            var device = new MuxerDevice() { Udid = "udid" };
            var pairingRecord = new PairingRecord();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    device,
                });
            muxer
                .Setup(m => m.ReadPairingRecordAsync("udid", default))
                .ReturnsAsync(pairingRecord);

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            using (var scope = await provider.CreateDeviceScopeAsync(null, default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
                Assert.Same(pairingRecord, context.PairingRecord);
            }
        }

        /// <summary>
        /// <see cref="ServiceProviderExtensions.CreateDeviceScopeAsync(IServiceProvider, string, CancellationToken)"/>
        /// throws an exception when the requested device is not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_Udid_NoMatch_ThrowsAsync()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice() { Udid = "1" },
                    new MuxerDevice() { Udid = "2" },
                });

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            await Assert.ThrowsAsync<MuxerException>(() => provider.CreateDeviceScopeAsync("3", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ServiceProviderExtensions.CreateDeviceScopeAsync(IServiceProvider, string, CancellationToken)"/>
        /// returns the requested device when available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateDeviceScopeAsync_Udid_Match_ReturnsDeviceAsync()
        {
            var device = new MuxerDevice() { Udid = "2" };
            var pairingRecord = new PairingRecord();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice() { Udid = "1" },
                    device,
                });
            muxer
                .Setup(m => m.ReadPairingRecordAsync("2", default))
                .ReturnsAsync(pairingRecord);

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            using (var scope = await provider.CreateDeviceScopeAsync("2", default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
                Assert.Same(pairingRecord, context.PairingRecord);
            }
        }

        /// <summary>
        /// The <see cref="ServiceProviderExtensions.StartServiceAsync{T}(IServiceProvider, string, CancellationToken)"/> method works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_Works_Async()
        {
            var device = new MuxerDevice() { Udid = "2" };
            var pairingRecord = new PairingRecord();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>() { device });
            muxer
                .Setup(m => m.ReadPairingRecordAsync("2", default))
                .ReturnsAsync(pairingRecord);

            var client = new Mock<LockdownClient>(MockBehavior.Strict);

            var factory = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            factory.Setup(f => f.CreateAsync(default)).ReturnsAsync(client.Object);

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .AddScoped<ClientFactory<LockdownClient>>((sp) => factory.Object)
                .BuildServiceProvider();

            using (var context = await provider.StartServiceAsync<LockdownClient>("2", default).ConfigureAwait(false))
            {
                Assert.Same(device, context.Device);
                Assert.NotNull(context.Scope);
                Assert.Same(client.Object, context.Service);
            }
        }
    }
}
