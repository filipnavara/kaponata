// <copyright file="ServiceCollectionExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.MobileImageMounter;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.NotificationProxy;
using Kaponata.iOS.PropertyLists;
using Microsoft.Extensions.DependencyInjection;

namespace Kaponata.iOS.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds iOS-related functionality to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add services to.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddAppleServices(this IServiceCollection services)
        {
            services.AddSingleton<MuxerClient>();
            services.AddSingleton<PairingRecordGenerator>();
            services.AddSingleton<DeviceServiceProvider>();

            services.AddScoped<DeviceContext>();

            services.AddTransient<ClientFactory<LockdownClient>, LockdownClientFactory>();
            services.AddTransient<ClientFactory<NotificationProxyClient>, NotificationProxyClientFactory>();
            services.AddTransient<ClientFactory<MobileImageMounterClient>, MobileImageMounterClientFactory>();
            services.AddTransient<PropertyListProtocolFactory>();

            return services;
        }
    }
}
