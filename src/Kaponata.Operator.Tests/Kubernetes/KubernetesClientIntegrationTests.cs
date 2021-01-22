// <copyright file="KubernetesClientIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Contains integration tests for the <see cref="KubernetesClient"/> class.
    /// </summary>
    public class KubernetesClientIntegrationTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClientIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public KubernetesClientIntegrationTests(ITestOutputHelper output)
        {
            this.output = output;
            this.loggerFactory = LogFactory.Create(output);
        }

        /// <summary>
        /// Runs an integration test for the <see cref="KubernetesClient.WaitForPodRunningAsync"/> method.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task WaitForPodRunning_IntegrationTest_Async()
        {
            using (var kubernetes = new KubernetesProtocol(
                KubernetesClientConfiguration.BuildDefaultConfig(),
                this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                this.loggerFactory))
            using (var client = new KubernetesClient(kubernetes, this.loggerFactory.CreateLogger<KubernetesClient>()))
            {
                V1Pod pod;

                if ((pod = await client.TryReadPodAsync("default", FormatName(nameof(this.WaitForPodRunning_IntegrationTest_Async)), default).ConfigureAwait(false)) != null)
                {
                    await client.DeletePodAsync(pod, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                pod = await client.CreatePodAsync(
                    new V1Pod()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = FormatName(nameof(this.WaitForPodRunning_IntegrationTest_Async)),
                            NamespaceProperty = "default",
                        },
                        Spec = new V1PodSpec()
                        {
                            Containers = new V1Container[]
                            {
                                new V1Container()
                                {
                                    Name = "nginx",
                                    Image = "nginx:1.19.6",
                                },
                            },
                        },
                    },
                    default).ConfigureAwait(false);

                await client.WaitForPodRunningAsync(pod, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
            }
        }

        private static string FormatName(string value)
        {
            return value.ToLower().Replace("_", "-");
        }
    }
}
