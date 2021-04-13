// <copyright file="ServiceProviderExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for the <see cref="IServiceProvider"/> interface.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Asynchronously create a device scope, which can be used to interact with services running on the iOS device.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="IServiceProvider"/> which provides the required services.
        /// </param>
        /// <param name="udid">
        /// The UDID of the device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="IServiceScope"/>
        /// from which device services (such as lockdown service clients) can be sourced.
        /// </returns>
        public static async Task<IServiceScope> CreateDeviceScopeAsync(this IServiceProvider provider, string udid, CancellationToken cancellationToken)
        {
            var muxer = provider.GetRequiredService<MuxerClient>();
            var allDevices = await muxer.ListDevicesAsync(cancellationToken).ConfigureAwait(false);

            // The UDID can be null, in which case we select the first device.
            var devices = allDevices.Where(
                d => udid == null
                || string.Equals(d.Udid, udid, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (devices.Length != 1)
            {
                throw new MuxerException($"Could not find the device with udid '{udid}'.");
            }

            var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
            context.Device = devices[0];

            return scope;
        }
    }
}
