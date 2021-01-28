// <copyright file="KubernetesClientIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            using (var client = this.CreateKubernetesClient())
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

            using (var client = this.CreateKubernetesClient())
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

            using (var client = this.CreateKubernetesClient())
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

        /// <summary>
        /// Runs an integration test which creates and deletes a <see cref="MobileDevice"/> object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task CreateDeleteMobileDevice_Async()
        {
            string name = FormatName(nameof(this.CreateDeleteMobileDevice_Async));

            using (var client = this.CreateKubernetesClient())
            {
                MobileDevice currentDevice = null;

                if ((currentDevice = await client.TryReadMobileDeviceAsync("default", name, default).ConfigureAwait(false)) != null)
                {
                    await client.DeleteMobileDeviceAsync(currentDevice, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                var device = new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        NamespaceProperty = "default",
                    },
                };

                var newDevice = await client.CreateMobileDeviceAsync(device, default).ConfigureAwait(false);
                Assert.NotNull(newDevice);

                var readDevice = await client.TryReadMobileDeviceAsync("default", name, default).ConfigureAwait(false);
                Assert.NotNull(readDevice);

                await client.DeleteMobileDeviceAsync(newDevice, TimeSpan.FromMinutes(1), default).ConfigureAwait(false);

                Assert.Null(await client.TryReadMobileDeviceAsync("default", name, default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Runs an integration test which watches a <see cref="MobileDevice"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task WatchMobileDevice_Async()
        {
            string name = FormatName(nameof(this.WatchMobileDevice_Async));

            using (var client = this.CreateKubernetesClient())
            {
                Collection<(WatchEventType, MobileDevice)> watchEvents = new Collection<(WatchEventType, MobileDevice)>();
                MobileDevice currentDevice = null;

                if ((currentDevice = await client.TryReadMobileDeviceAsync("default", name, default).ConfigureAwait(false)) != null)
                {
                    await client.DeleteMobileDeviceAsync(currentDevice, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                var watch = client.WatchMobileDeviceAsync(
                    new MobileDevice() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default", Name = name } },
                    (type, device) =>
                    {
                        watchEvents.Add((type, device));

                        return Task.FromResult(type == WatchEventType.Deleted ? WatchResult.Stop : WatchResult.Continue);
                    },
                    default);

                var device = new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        NamespaceProperty = "default",
                    },
                };

                await client.CreateMobileDeviceAsync(device, default).ConfigureAwait(false);
                await client.DeleteMobileDeviceAsync(device, TimeSpan.FromMinutes(1), default).ConfigureAwait(false);
                await watch.ConfigureAwait(false);

                Assert.Collection(
                    watchEvents,
                    e =>
                    {
                        Assert.Equal(WatchEventType.Added, e.Item1);
                        Assert.NotNull(e.Item2);
                    },
                    e =>
                    {
                        Assert.Equal(WatchEventType.Deleted, e.Item1);
                        Assert.NotNull(e.Item2);
                    });
            }
        }

        /// <summary>
        /// Runs an integration test which creates and deletes a <see cref="WebDriverSession"/> object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task CreateDeleteWebDriverSession_Async()
        {
            string name = FormatName(nameof(this.CreateDeleteWebDriverSession_Async));

            using (var client = this.CreateKubernetesClient())
            {
                WebDriverSession currentDevice = null;

                if ((currentDevice = await client.TryReadWebDriverSessionAsync("default", name, default).ConfigureAwait(false)) != null)
                {
                    await client.DeleteWebDriverSessionAsync(currentDevice, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                var session = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        NamespaceProperty = "default",
                    },
                };

                var newSession = await client.CreateWebDriverSessionAsync(session, default).ConfigureAwait(false);
                Assert.NotNull(newSession);

                var readSession = await client.TryReadWebDriverSessionAsync("default", name, default).ConfigureAwait(false);
                Assert.NotNull(readSession);

                await client.DeleteWebDriverSessionAsync(newSession, TimeSpan.FromMinutes(1), default).ConfigureAwait(false);

                Assert.Null(await client.TryReadWebDriverSessionAsync("default", name, default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Runs an integration test which watches a <see cref="WebDriverSession"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task WatchWebDriverSession_Async()
        {
            string name = FormatName(nameof(this.WatchWebDriverSession_Async));

            using (var client = this.CreateKubernetesClient())
            {
                var watchEvents = new Collection<(WatchEventType, WebDriverSession)>();
                WebDriverSession currentSession = null;

                if ((currentSession = await client.TryReadWebDriverSessionAsync("default", name, default).ConfigureAwait(false)) != null)
                {
                    await client.DeleteWebDriverSessionAsync(currentSession, TimeSpan.FromSeconds(100), default).ConfigureAwait(false);
                }

                var watch = client.WatchWebDriverSessionAsync(
                    new WebDriverSession() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default", Name = name } },
                    (type, device) =>
                    {
                        watchEvents.Add((type, device));

                        return Task.FromResult(type == WatchEventType.Deleted ? WatchResult.Stop : WatchResult.Continue);
                    },
                    default);

                var session = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = name,
                        NamespaceProperty = "default",
                    },
                };

                await client.CreateWebDriverSessionAsync(session, default).ConfigureAwait(false);
                await client.DeleteWebDriverSessionAsync(session, TimeSpan.FromMinutes(1), default).ConfigureAwait(false);
                await watch.ConfigureAwait(false);

                Assert.Collection(
                    watchEvents,
                    e =>
                    {
                        Assert.Equal(WatchEventType.Added, e.Item1);
                        Assert.NotNull(e.Item2);
                    },
                    e =>
                    {
                        Assert.Equal(WatchEventType.Deleted, e.Item1);
                        Assert.NotNull(e.Item2);
                    });
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

        private KubernetesClient CreateKubernetesClient()
        {
            return new KubernetesClient(
                new KubernetesProtocol(
                    KubernetesClientConfiguration.BuildDefaultConfig(),
                    this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                    this.loggerFactory),
                this.loggerFactory.CreateLogger<KubernetesClient>(),
                this.loggerFactory);
        }
    }
}
