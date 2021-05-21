// <copyright file="DeviceServiceProvider.cs" company="Quamotion bv">
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
    /// A <see cref="DeviceServiceProvider"/> is a specialized <see cref="IServiceProvider"/>, which allows creating device scopes,
    /// and connecting to services running on devices.
    /// </summary>
    public class DeviceServiceProvider
    {
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceProvider"/> class.
        /// </summary>
        /// <param name="provider">
        /// The underlying <see cref="IServiceProvider"/> which provides iOS-related services.
        /// </param>
        public DeviceServiceProvider(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceProvider"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
        protected DeviceServiceProvider()
        {
        }

        /// <summary>
        /// Asynchronously create a device scope, which can be used to interact with services running on the iOS device.
        /// </summary>
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
        public virtual async Task<IServiceScope> CreateDeviceScopeAsync(string udid, CancellationToken cancellationToken)
        {
            var muxer = this.provider.GetRequiredService<MuxerClient>();
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

            var scope = this.provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
            context.Device = devices[0];

            context.PairingRecord = await muxer.ReadPairingRecordAsync(context.Device.Udid, cancellationToken).ConfigureAwait(false);

            return scope;
        }

        /// <summary>
        /// Asynchronously creates a device scope and starts a service on a device.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the service to start.
        /// </typeparam>
        /// <param name="udid">
        /// The UDID of the device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and, when completed,
        /// returns a <see cref="DeviceServiceScope{T}"/> which provides access to the service running on the device.
        /// </returns>
        public virtual async Task<DeviceServiceScope<T>> StartServiceAsync<T>(string udid, CancellationToken cancellationToken)
        {
            var scope = await this.CreateDeviceScopeAsync(udid, cancellationToken).ConfigureAwait(false);
            var service = await scope.StartServiceAsync<T>(cancellationToken).ConfigureAwait(false);

            return new DeviceServiceScope<T>(
                scope,
                device: scope.ServiceProvider.GetRequiredService<DeviceContext>().Device,
                service);
        }
    }
}
