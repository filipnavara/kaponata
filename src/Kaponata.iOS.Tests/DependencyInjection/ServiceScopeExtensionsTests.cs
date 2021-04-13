// <copyright file="ServiceScopeExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="ServiceScopeExtensions"/> class.
    /// </summary>
    public class ServiceScopeExtensionsTests
    {
        /// <summary>
        /// <see cref="ServiceScopeExtensions.StartServiceAsync{T}(IServiceScope, CancellationToken)"/> returns a service client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task StartServiceAsync_Works_Async()
        {
            var lockdown = new Mock<LockdownClient>(MockBehavior.Strict);

            var lockdownFactory = new Mock<ClientFactory<LockdownClient>>(MockBehavior.Strict);
            lockdownFactory.Setup(l => l.CreateAsync(default)).ReturnsAsync(lockdown.Object);

            var provider = new ServiceCollection()
                .AddScoped<DeviceContext>()
                .AddScoped<ClientFactory<LockdownClient>>((sp) => lockdownFactory.Object)
                .BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                Assert.Equal(lockdown.Object, await scope.StartServiceAsync<LockdownClient>(default).ConfigureAwait(false));
            }
        }
    }
}
