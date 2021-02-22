// <copyright file="KubernetesAdbContextTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Android.Adb;
using Kaponata.Kubernetes;
using Kaponata.Operator.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="KubernetesAdbContext"/> class.
    /// </summary>
    public class KubernetesAdbContextTests
    {
        /// <summary>
        /// A <see cref="AdbClient"/> object can be sourced from a dependency injection container, and uses a <see cref="KubernetesAdbSocketLocator"/>
        /// configured in that DI container if one is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task KubernetesAdbSocketLocator_WorksWithDependencyInjection_Async()
        {
            var pod = new V1Pod();

            var kubernetes = new Mock<KubernetesClient>();
            var context = new KubernetesAdbContext() { Pod = pod };
            var stream = Mock.Of<Stream>();

            kubernetes
                .Setup(p => p.ConnectToPodPortAsync(pod, 5037, default))
                .ReturnsAsync(stream);

            var services =
                new ServiceCollection()
                .AddScoped<AdbClient>()
                .AddScoped<AdbSocketLocator, KubernetesAdbSocketLocator>()
                .AddScoped((p) => context)
                .AddSingleton(kubernetes.Object)
                .AddLogging()
                .BuildServiceProvider();

            var client = services.GetRequiredService<AdbClient>();
            await using (var protocol = await client.TryConnectToAdbAsync(default))
            {
                Assert.Same(stream, protocol.Stream);
            }
        }
    }
}
