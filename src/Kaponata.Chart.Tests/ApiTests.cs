// <copyright file="ApiTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// Tests the API server.
    /// </summary>
    public class ApiTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public ApiTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// At least one API server pod is running in the cluster.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task ApiServer_Is_Running_Async()
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

        /// <summary>
        /// The ingress for the API server works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task ApiServer_Ingress_Works_Async()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://localhost:80/");

                var response = await httpClient.GetAsync("/api").ConfigureAwait(false);

                // The /api endpoint itself does not exist, so the API server will return a 404 status
                // code, _but_ we will get the X-Kaponata-Version header which we can use to verify
                // we are indeed talking to the operator.
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.True(response.Headers.TryGetValues("X-Kaponata-Version", out var values));
                Assert.Equal(ThisAssembly.AssemblyInformationalVersion, Assert.Single(values));
            }
        }
    }
}
