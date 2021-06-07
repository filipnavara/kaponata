// <copyright file="LicenseStoreTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.Licensing;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Licensing
{
    /// <summary>
    /// Tests the <see cref="LicenseStore"/> class.
    /// </summary>
    public class LicenseStoreTests
    {
        /// <summary>
        /// The <see cref="LicenseStore"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new LicenseStore(null, NullLogger<LicenseStore>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LicenseStore(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="LicenseStore.AddLicenseAsync(XDocument, System.Threading.CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddLicenseAsync_ValidatesArguments_Async()
        {
            var store = new LicenseStore(Mock.Of<KubernetesClient>(), NullLogger<LicenseStore>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddLicenseAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="LicenseStore.AddLicenseAsync(XDocument, CancellationToken)"/> creates a new config map if no config map is present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddLicenseAsync_AddsNew_Async()
        {
            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var configClient = new Mock<NamespacedKubernetesClient<V1ConfigMap>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1ConfigMap>()).Returns(configClient.Object);
            configClient.Setup(c => c.TryReadAsync("kaponata-license", default)).ReturnsAsync((V1ConfigMap)null).Verifiable();
            configClient.Setup(c => c.CreateAsync(It.IsAny<V1ConfigMap>(), default)).Returns(Task.FromResult(new V1ConfigMap())).Verifiable();

            var store = new LicenseStore(client.Object, NullLogger<LicenseStore>.Instance);

            var license = new XDocument();

            await store.AddLicenseAsync(license, default).ConfigureAwait(false);

            configClient.Verify();
        }

        /// <summary>
        /// <see cref="LicenseStore.AddLicenseAsync(XDocument, CancellationToken)"/> updates the existing config map if a config map is present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddLicenseAsync_UpdatesExisting_Async()
        {
            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var configClient = new Mock<NamespacedKubernetesClient<V1ConfigMap>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1ConfigMap>()).Returns(configClient.Object);
            var configMap = new V1ConfigMap();
            configClient.Setup(c => c.TryReadAsync("kaponata-license", default)).ReturnsAsync(configMap).Verifiable();
            configClient.Setup(c => c.PatchAsync(configMap, It.IsAny<JsonPatchDocument<V1ConfigMap>>(), default)).ReturnsAsync(configMap).Verifiable();

            var store = new LicenseStore(client.Object, NullLogger<LicenseStore>.Instance);

            var license = new XDocument();

            await store.AddLicenseAsync(license, default).ConfigureAwait(false);

            configClient.Verify();
        }

        /// <summary>
        /// <see cref="LicenseStore.GetLicenseAsync(CancellationToken)"/> returns <see langword="null"/> if no license exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLicenseAsync_NoLicense_ReturnsNull_Async()
        {
            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var configClient = new Mock<NamespacedKubernetesClient<V1ConfigMap>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1ConfigMap>()).Returns(configClient.Object);
            configClient.Setup(c => c.TryReadAsync("kaponata-license", default)).ReturnsAsync((V1ConfigMap)null);

            var store = new LicenseStore(client.Object, NullLogger<LicenseStore>.Instance);

            Assert.Null(await store.GetLicenseAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="LicenseStore.GetLicenseAsync(CancellationToken)"/> returns the currently available license.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLicenseAsync_ReturnsLicense_Async()
        {
            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var configClient = new Mock<NamespacedKubernetesClient<V1ConfigMap>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1ConfigMap>()).Returns(configClient.Object);
            var configMap = new V1ConfigMap()
            {
                Data = new Dictionary<string, string>(),
            };
            configMap.Data.Add("license", "<license></license>");
            configClient.Setup(c => c.TryReadAsync("kaponata-license", default)).ReturnsAsync(configMap);

            var store = new LicenseStore(client.Object, NullLogger<LicenseStore>.Instance);

            Assert.NotNull(await store.GetLicenseAsync(default).ConfigureAwait(false));
        }
    }
}
