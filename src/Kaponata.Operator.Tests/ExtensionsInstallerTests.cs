// <copyright file="ExtensionsInstallerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests
{
    /// <summary>
    /// Tests the <see cref="ExtensionsInstaller"/> class.
    /// </summary>
    public class ExtensionsInstallerTests
    {
        /// <summary>
        /// The <see cref="ExtensionsInstaller"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("kubernetesClient", () => new ExtensionsInstaller(null, NullLogger<ExtensionsInstaller>.Instance, new TestConsole()));
            Assert.Throws<ArgumentNullException>("logger", () => new ExtensionsInstaller(Mock.Of<KubernetesClient>(), null, new TestConsole()));
            Assert.Throws<ArgumentNullException>("console", () => new ExtensionsInstaller(Mock.Of<KubernetesClient>(), NullLogger<ExtensionsInstaller>.Instance, null));
        }

        /// <summary>
        /// The <see cref="ExtensionsInstaller.RunAsync(IHost)"/> uses the services from the host and installs the CRD.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task RunAsync_InstallsCrd_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes
                .Setup(k => k.InstallOrUpgradeCustomResourceDefinitionAsync(It.IsAny<V1CustomResourceDefinition>(), TimeSpan.FromMinutes(1), default))
                .Returns<V1CustomResourceDefinition, TimeSpan, CancellationToken>(
                (crd, timeout, ct) =>
                {
                    return Task.FromResult(crd);
                });

            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddSingleton<IConsole, TestConsole>();
                    services.AddSingleton<KubernetesClient>(kubernetes.Object);
                    services.AddLogging();
                    services.AddSingleton<ExtensionsInstaller>();
                });
            var host = builder.Build();

            await ExtensionsInstaller.RunAsync(host);

            kubernetes.Verify();
        }
    }
}
