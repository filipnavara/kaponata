// <copyright file="GuacamoleTests.cs" company="Quamotion bv">
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
    /// Integration tests for the Guacamole chart.
    /// </summary>
    public class GuacamoleTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuacamoleTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public GuacamoleTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// At least one guacd pod is up and running, and can successfully perform a handshake.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Guacd_PerformsHandshake_Async()
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
                // There's at least one guacd pod
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/name=guacamole,app.kubernetes.io/component=guacd");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(5), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // Try to perform a handshake
                using (var connection = await client.ConnectToPodPortAsync(pod, 4822, default).ConfigureAwait(false))
                {
                    GuacdProtocol protocol = new GuacdProtocol(connection);

                    // Handshake phase
                    await protocol.SendInstructionAsync("select", new string[] { "vnc" }, default).ConfigureAwait(false);
                    var response = await protocol.ReadInstructionAsync(default);

                    Assert.Equal("args", response[0]);
                    Assert.Equal("VERSION_1_3_0", response[1]);
                }
            }
        }

        /// <summary>
        /// At least one Guacamole pod is up and running, and the user can request a token.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Guacamole_CanAuthenticate_OnPod_Async()
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
                // There's at least one guacd pod
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/name=guacamole,app.kubernetes.io/component=guacamole");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(5), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // Try to perform a handshake
                var httpClient = client.CreatePodHttpClient(pod, 8080);
                httpClient.BaseAddress = new Uri($"http://{pod.Metadata.Name}:8080/guacamole/");
                var guacamoleClient = new GuacamoleClient(httpClient);

                var token = await guacamoleClient.GenerateTokenAsync(default).ConfigureAwait(false);

                Assert.NotNull(token.AuthToken);
                Assert.Equal("RestAuthProvider", token.DataSource);
                Assert.Single(token.AvailableDataSources, "RestAuthProvider");
                Assert.NotNull(token.UserName);
            }
        }

        /// <summary>
        /// The Guacamole service is exposed over the default ingress, the user can successfully authenticate, and fetch an (empty)
        /// connection tree.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Guacamole_CanAuthenticate_OverIngress_Async()
        {
            // Try to perform a handshake
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"http://localhost:80/guacamole/");
            var guacamoleClient = new GuacamoleClient(httpClient);

            var token = await guacamoleClient.GenerateTokenAsync(default).ConfigureAwait(false);

            Assert.NotNull(token.AuthToken);
            Assert.Equal("RestAuthProvider", token.DataSource);
            Assert.Single(token.AvailableDataSources, "RestAuthProvider");
            Assert.NotNull(token.UserName);

            var tree = await guacamoleClient.GetTreeAsync("RestAuthProvider", token.AuthToken, default).ConfigureAwait(false);
            Assert.Equal(0, tree.ActiveConnections);
            Assert.Equal("ROOT", tree.Identifier);
            Assert.Equal("ROOT", tree.Name);
            Assert.Equal("ORGANIZATIONAL", tree.Type);
        }
    }
}
