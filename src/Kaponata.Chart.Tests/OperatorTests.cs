// <copyright file="OperatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// Tests the deployment of the Kaponata operator into the cluster.
    /// </summary>
    public class OperatorTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public OperatorTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// At least one Operator pod is running in the cluster.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Operator_Is_Running_Async()
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
                // There's at least one operator pod
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=operator");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(5), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // We can connect to port 80 and fetch the status
                using (var httpClient = new HttpClient(
                    new SocketsHttpHandler()
                    {
                         ConnectCallback = (context, cancellationToken) => client.ConnectToPodPortAsync(pod, 80, cancellationToken),
                    }))
                {
                    httpClient.BaseAddress = new Uri("http://localhost:80/");

                    var urls = new string[] { "/", "/health/ready", "/health/alive" };

                    foreach (var url in urls)
                    {
                        var response = await httpClient.GetAsync(url).ConfigureAwait(false);
                        Assert.True(response.IsSuccessStatusCode);
                        Assert.True(response.Headers.TryGetValues("X-Kaponata-Version", out var values));
                        Assert.Equal(ThisAssembly.AssemblyInformationalVersion, Assert.Single(values));
                    }
                }
            }
        }
    }
}
