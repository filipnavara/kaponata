// <copyright file="ServiceProviderExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
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
            var device = new MuxerDevice();

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    device,
                });

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            using (var scope = await provider.CreateDeviceScopeAsync(null, default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
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

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>()
                {
                    new MuxerDevice() { Udid = "1" },
                    device,
                });

            var provider = new ServiceCollection()
                .AddSingleton<MuxerClient>(muxer.Object)
                .AddScoped<DeviceContext>()
                .BuildServiceProvider();

            using (var scope = await provider.CreateDeviceScopeAsync("2", default).ConfigureAwait(false))
            {
                var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
                Assert.Same(device, context.Device);
            }
        }
    }
}
