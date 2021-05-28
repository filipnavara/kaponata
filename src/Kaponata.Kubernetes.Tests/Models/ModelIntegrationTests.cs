// <copyright file="ModelIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Kubernetes.Tests.Models
{
    /// <summary>
    /// Integration tests for the <see cref="MobileDevice"/> model.
    /// </summary>
    public class ModelIntegrationTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public ModelIntegrationTests(ITestOutputHelper output)
        {
            this.output = output;
            this.loggerFactory = LogFactory.Create(output);
        }

        /// <summary>
        /// Tests updating the condition of a mobile device's status.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task UpdateDeviceConditionAsync()
        {
            using (var client = this.CreateKubernetesClient())
            {
                var deviceClient = client.GetClient<MobileDevice>();

                await deviceClient.TryDeleteAsync(nameof(this.UpdateDeviceConditionAsync).ToLower(), timeout: TimeSpan.FromMinutes(1), default);

                var device = await deviceClient.CreateAsync(
                    new MobileDevice()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = nameof(this.UpdateDeviceConditionAsync).ToLower(),
                            NamespaceProperty = "default",
                        },
                    },
                    default);

                device.Status = new MobileDeviceStatus();
                Assert.True(device.Status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.Unknown, "NotMounted", "Not Mounted"));
                var patch = new JsonPatchDocument<MobileDevice>();
                patch.Add(d => d.Status, device.Status);

                device = await deviceClient.PatchStatusAsync(device, patch, default).ConfigureAwait(false);
                Assert.Equal(ConditionStatus.Unknown, device.Status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));

                Assert.True(device.Status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.True, "NotMounted", "Not Mounted"));
                patch.Add(d => d.Status, device.Status);
                device = await deviceClient.PatchStatusAsync(device, patch, default).ConfigureAwait(false);
                Assert.Equal(ConditionStatus.True, device.Status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));
            }
        }

        private KubernetesClient CreateKubernetesClient()
        {
            return new KubernetesClient(
                new KubernetesProtocol(
                    KubernetesClientConfiguration.BuildDefaultConfig(),
                    this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                    this.loggerFactory),
                KubernetesOptions.Default,
                this.loggerFactory.CreateLogger<KubernetesClient>(),
                this.loggerFactory);
        }
    }
}
