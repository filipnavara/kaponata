// <copyright file="UsbmuxdTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Divergic.Logging.Xunit;
using k8s;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Polyfill;
using Kaponata.Operator.Kubernetes;
using Microsoft.Extensions.Logging;
using System;
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
        /// At least one usbmuxd pod is running in the cluster and we can fetch a device list.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Usbmuxd_CanListDevices_Async()
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
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=usbmuxd");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(2), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // We can connect to port 27015 and retrieve an empty device list
                var locator = new KubernetesMuxerSocketLocator(kubernetes, pod, this.loggerFactory.CreateLogger<KubernetesMuxerSocketLocator>(), this.loggerFactory);
                var muxerClient = new MuxerClient(this.loggerFactory.CreateLogger<MuxerClient>(), this.loggerFactory, locator);

                var devices = await muxerClient.ListDevicesAsync(default).ConfigureAwait(false);
                Assert.Empty(devices);
            }
        }

        /// <summary>
        /// At least one usbmuxd pod is running in the cluster and reports the current System Buid.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Usbmuxd_HasGlobalSystemId_Async()
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
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=usbmuxd");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(2), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // We can fetch the system buid, and the system buid reported by usbmuxd is the system buid stored in the SystemConfiguration.plist
                // value in the usbmuxd configmap
                var locator = new KubernetesMuxerSocketLocator(kubernetes, pod, this.loggerFactory.CreateLogger<KubernetesMuxerSocketLocator>(), this.loggerFactory);
                var muxerClient = new MuxerClient(this.loggerFactory.CreateLogger<MuxerClient>(), this.loggerFactory, locator);
                var systemBuid = await muxerClient.ReadBuidAsync(default).ConfigureAwait(false);

                var configMaps = await kubernetes.ListNamespacedConfigMapAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=usbmuxd");
                var configMap = Assert.Single(configMaps.Items);

                var xml = configMap.Data["SystemConfiguration.plist"];
                var dict = (NSDictionary)XmlPropertyListParser.ParseString(xml);
                var configValue = dict["SystemBUID"].ToObject();

                Assert.Equal(configValue, systemBuid);
            }
        }

        /// <summary>
        /// The usbmuxd pod supports reading and writing pairing records.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Usbmuxd_CanReadWritePairingRecord_Async()
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
                var pods = await kubernetes.ListNamespacedPodAsync(config.Namespace, labelSelector: "app.kubernetes.io/component=usbmuxd");
                Assert.NotEmpty(pods.Items);
                var pod = pods.Items[0];

                // The pod is in the running state
                pod = await client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(2), default).ConfigureAwait(false);
                Assert.Equal("Running", pod.Status.Phase);

                // We can fetch the system buid, and the system buid reported by usbmuxd is the system buid stored in the SystemConfiguration.plist
                // value in the usbmuxd configmap
                var locator = new KubernetesMuxerSocketLocator(kubernetes, pod, this.loggerFactory.CreateLogger<KubernetesMuxerSocketLocator>(), this.loggerFactory);
                var muxerClient = new MuxerClient(this.loggerFactory.CreateLogger<MuxerClient>(), this.loggerFactory, locator);

                var udid = Guid.NewGuid().ToString();
                var systemBuid = Guid.NewGuid().ToString();
                var key = Convert.FromBase64String(
                    "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                    "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                    "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                    "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                    "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

                // usbmuxd initially won't contain a pairing record for this device
                var pairingRecord = await muxerClient.ReadPairingRecordAsync(udid, default).ConfigureAwait(false);
                Assert.Null(pairingRecord);

                pairingRecord = new PairingRecordGenerator().Generate(key, systemBuid);
                await muxerClient.SavePairingRecordAsync(udid, pairingRecord, default).ConfigureAwait(false);

                var muxerPairingRecord = await muxerClient.ReadPairingRecordAsync(udid, default).ConfigureAwait(false);
                Assert.Equal(pairingRecord.ToByteArray(), muxerPairingRecord.ToByteArray());
            }
        }
    }
}
