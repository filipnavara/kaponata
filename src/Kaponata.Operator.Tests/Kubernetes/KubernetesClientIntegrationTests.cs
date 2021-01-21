﻿using Divergic.Logging.Xunit;
using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var pods = await kubernetes.ListNamespacedPodAsync("default", fieldSelector: $"metadata.name={FormatName(nameof(this.WaitForPodRunning_IntegrationTest_Async))}").ConfigureAwait(false);

                foreach (var podToDelete in pods.Items)
                {
                    await client.DeletePodAsync(podToDelete, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                var pod = await kubernetes.CreateNamespacedPodAsync(
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
                    "default").ConfigureAwait(false);

                await client.WaitForPodRunningAsync(pod, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
            }
        }

        private static string FormatName(string value)
        {
            return value.ToLower().Replace("_", "-");
        }
    }
}