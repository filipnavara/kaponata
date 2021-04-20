// <copyright file="RegistryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// Tests the Registry deployment.
    /// </summary>
    public class RegistryTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public RegistryTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// At least one registry pod is running and responding to HTTP requests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Registry_Running_Async()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            if (config.Namespace == null)
            {
                config.Namespace = "default";
            }

            using (var kubernetes = new KubernetesProtocol(
                config,
                this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                this.loggerFactory))
            using (var client = new KubernetesClient(
                kubernetes,
                KubernetesOptions.Default,
                this.output.BuildLoggerFor<KubernetesClient>(),
                this.loggerFactory))
            {
                // There's at least one usbmuxd pod
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=registry");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(2), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // Try to perform a handshake
                var httpClient = client.CreatePodHttpClient(pod, 5000);
                httpClient.BaseAddress = new Uri($"http://{pod.Metadata.Name}:5000/v2/");

                var response = await httpClient.GetAsync(string.Empty).ConfigureAwait(false);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var apiVersion = Assert.Single(response.Headers.GetValues("Docker-Distribution-Api-Version"));
                Assert.Equal("registry/2.0", apiVersion);
            }
        }
    }
}
