// <copyright file="UsbmuxdTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using Kaponata.iOS.Muxer;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// Tests the usbmuxd Helm chart.
    /// </summary>
    public class UsbmuxdTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbmuxdTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public UsbmuxdTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// At least one usbmuxd pod is running in the cluster.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Usbmuxd_Is_Running_Async()
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
                // There's at least one usbmuxd pod
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=usbmuxd");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(5), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // We can connect to port 27015 and retrieve an empty device list
                var locator = new KubernetesMuxerSocketLocator(kubernetes, pod, this.loggerFactory.CreateLogger<KubernetesMuxerSocketLocator>(), this.loggerFactory);
                var muxerClient = new MuxerClient(this.loggerFactory.CreateLogger<MuxerClient>(), this.loggerFactory, locator);

                Exception exception = null;

                // The usbmuxd port may not yet be ready; in that case an IOException is thrown.
                // Try up to 10 times.
                for (int i = 0; i < 10; i++)
                {
                    exception = null;

                    try
                    {
                        var devices = await muxerClient.ListDevicesAsync(default).ConfigureAwait(false);
                        break;
                    }
                    catch (IOException ex)
                    {
                        exception = ex;
                        this.output.WriteLine(ex.Message);
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    }
                }

                if (exception != null)
                {
                    throw exception;
                }
            }
        }
    }
}
