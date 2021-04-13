// <copyright file="ServiceCollectionExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.iOS.NotificationProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kaponata.iOS.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// The <see cref="ServiceCollectionExtensions.AddAppleServices(IServiceCollection)"/> method properly configures
        /// the service collection.
        /// </summary>
        [Fact]
        public void AddAppleServices_Works()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddAppleServices()
                .BuildServiceProvider();

            // Make sure we can get the most common types
            var muxer = serviceProvider.GetRequiredService<MuxerClient>();
            var lockdownFactory = serviceProvider.GetRequiredService<ClientFactory<LockdownClient>>();
            var notificationProxyClientFactory = serviceProvider.GetRequiredService<ClientFactory<NotificationProxyClient>>();
        }
    }
}
