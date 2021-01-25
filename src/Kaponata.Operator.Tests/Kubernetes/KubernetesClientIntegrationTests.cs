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
using System.Collections.Generic;
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
            using (var client = new KubernetesClient(kubernetes, this.loggerFactory.CreateLogger<KubernetesClient>(), this.loggerFactory))
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

        /// <summary>
        /// Runs an integration test which creates and then deletes a CRD.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task CreateDeleteCrd_IntegrationTest_Async()
        {
            string name = $"{FormatName(nameof(this.CreateDeleteCrd_IntegrationTest_Async))}s.kaponata.io";

            using (var kubernetes = new KubernetesProtocol(
                KubernetesClientConfiguration.BuildDefaultConfig(),
                this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                this.loggerFactory))
            using (var client = new KubernetesClient(kubernetes, this.loggerFactory.CreateLogger<KubernetesClient>(), this.loggerFactory))
            {
                V1CustomResourceDefinition crd;

                if ((crd = await client.TryReadCustomResourceDefinitionAsync(name, default).ConfigureAwait(false)) != null)
                {
                    await client.DeleteCustomResourceDefinitionAsync(crd, TimeSpan.FromMinutes(1), default).ConfigureAwait(false);
                }

                crd = await client.CreateCustomResourceDefinitionAsync(
                    new V1CustomResourceDefinition()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = name,
                        },
                        Spec = new V1CustomResourceDefinitionSpec()
                        {
                            Group = "kaponata.io",
                            Scope = "Namespaced",
                            Names = new V1CustomResourceDefinitionNames()
                            {
                                Plural = $"{FormatName(nameof(this.CreateDeleteCrd_IntegrationTest_Async))}s",
                                Singular = FormatName(nameof(this.CreateDeleteCrd_IntegrationTest_Async)),
                                Kind = FormatNameCamelCase(nameof(this.CreateDeleteCrd_IntegrationTest_Async)),
                            },
                            Versions = new V1CustomResourceDefinitionVersion[]
                            {
                                new V1CustomResourceDefinitionVersion()
                                {
                                    Name = "v1alpha1",
                                    Served = true,
                                    Storage = true,
                                    Schema = new V1CustomResourceValidation()
                                    {
                                         OpenAPIV3Schema = new V1JSONSchemaProps()
                                         {
                                             Type = "object",
                                         },
                                    },
                                },
                            },
                        },
                    },
                    default).ConfigureAwait(false);

                Assert.NotNull(await client.TryReadCustomResourceDefinitionAsync(crd.Metadata.Name, default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Runs an integration test which installs, upgrades and then deletes a CRD.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task InstallUpgradeDeleteCrd_IntegrationTest_Async()
        {
            string name = $"{FormatName(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async))}s.kaponata.io";

            using (var kubernetes = new KubernetesProtocol(
                KubernetesClientConfiguration.BuildDefaultConfig(),
                this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                this.loggerFactory))
            using (var client = new KubernetesClient(kubernetes, this.loggerFactory.CreateLogger<KubernetesClient>(), this.loggerFactory))
            {
                var v1crd = new V1CustomResourceDefinition()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        Labels = new Dictionary<string, string>()
                        {
                            { Annotations.Version, "0.1" },
                        },
                    },
                    Spec = new V1CustomResourceDefinitionSpec()
                    {
                        Group = "kaponata.io",
                        Scope = "Namespaced",
                        Names = new V1CustomResourceDefinitionNames()
                        {
                            Plural = $"{FormatName(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async))}s",
                            Singular = FormatName(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async)),
                            Kind = FormatNameCamelCase(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async)),
                        },
                        Versions = new V1CustomResourceDefinitionVersion[]
                        {
                                new V1CustomResourceDefinitionVersion()
                                {
                                    Name = "v1alpha1",
                                    Served = true,
                                    Storage = true,
                                    Schema = new V1CustomResourceValidation()
                                    {
                                         OpenAPIV3Schema = new V1JSONSchemaProps()
                                         {
                                             Type = "object",
                                         },
                                    },
                                },
                        },
                    },
                };

                var v2crd = new V1CustomResourceDefinition()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        Labels = new Dictionary<string, string>()
                        {
                            { Annotations.Version, "0.2" },
                        },
                    },
                    Spec = new V1CustomResourceDefinitionSpec()
                    {
                        Group = "kaponata.io",
                        Scope = "Namespaced",
                        Names = new V1CustomResourceDefinitionNames()
                        {
                            Plural = $"{FormatName(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async))}s",
                            Singular = FormatName(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async)),
                            Kind = FormatNameCamelCase(nameof(this.InstallUpgradeDeleteCrd_IntegrationTest_Async)),
                        },
                        Versions = new V1CustomResourceDefinitionVersion[]
                        {
                                new V1CustomResourceDefinitionVersion()
                                {
                                    Name = "v1alpha1",
                                    Served = true,
                                    Storage = true,
                                    Schema = new V1CustomResourceValidation()
                                    {
                                         OpenAPIV3Schema = new V1JSONSchemaProps()
                                         {
                                             Type = "object",
                                         },
                                    },
                                },
                        },
                    },
                };

                var installedCrd = await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    v1crd,
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);

                installedCrd = await client.WaitForCustomResourceDefinitionEstablishedAsync(
                    installedCrd,
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);

                Assert.Equal("0.1", installedCrd.Metadata.Labels[Annotations.Version]);

                // Re-read to be sure.
                installedCrd = await client.TryReadCustomResourceDefinitionAsync(name, default);
                Assert.Equal("0.1", installedCrd.Metadata.Labels[Annotations.Version]);

                installedCrd = await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    v2crd,
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);

                installedCrd = await client.WaitForCustomResourceDefinitionEstablishedAsync(
                    installedCrd,
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);

                Assert.Equal("0.2", installedCrd.Metadata.Labels[Annotations.Version]);

                // Re-read to be sure.
                installedCrd = await client.TryReadCustomResourceDefinitionAsync(name, default);
                Assert.Equal("0.2", installedCrd.Metadata.Labels[Annotations.Version]);

                await client.DeleteCustomResourceDefinitionAsync(
                    installedCrd,
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);
            }
        }

        private static string FormatNameCamelCase(string value)
        {
            return value.Replace("_", "-");
        }

        private static string FormatName(string value)
        {
            return value.ToLower().Replace("_", "-");
        }
    }
}
