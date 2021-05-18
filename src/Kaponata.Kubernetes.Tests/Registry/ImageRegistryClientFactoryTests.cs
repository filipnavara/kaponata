// <copyright file="ImageRegistryClientFactoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.Registry;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="ImageRegistryClientFactory"/> class.
    /// </summary>
    public class ImageRegistryClientFactoryTests
    {
        /// <summary>
        /// The <see cref="ImageRegistryClientFactory"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ImageRegistryClientFactory(null, new ImageRegistryClientConfiguration("test", 0)));
            Assert.Throws<ArgumentNullException>(() => new ImageRegistryClientFactory(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="ImageRegistryClientFactory.CreateAsync(CancellationToken)"/> returns a <see cref="HttpClient"/> which connects directly
        /// to the service when the code is running in a pod in a Kubernetes cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_InsideCluster_ConnectsDirectly_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(k => k.RunningInCluster).Returns(true);
            var configuration = new ImageRegistryClientConfiguration("registry", 5000);

            var factory = new ImageRegistryClientFactory(kubernetes.Object, configuration);

            var client = await factory.CreateAsync(default).ConfigureAwait(false);

            Assert.NotNull(client.HttpClient);
            Assert.Equal(new Uri("http://registry:5000"), client.HttpClient.BaseAddress);
        }

        /// <summary>
        /// <see cref="ImageRegistryClientFactory.CreateAsync(CancellationToken)"/> returns a <see cref="HttpClient"/> which connects
        /// to a pod backing the service, using port forwarding, when the code is not running in a pod in a Kubernetes cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_OutsideCluster_UsesPortForwarding_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(k => k.RunningInCluster).Returns(false);

            var serviceClient = new Mock<NamespacedKubernetesClient<V1Service>>();
            kubernetes.Setup(k => k.GetClient<V1Service>()).Returns(serviceClient.Object);

            serviceClient
                .Setup(s => s.TryReadAsync("registry", default))
                .ReturnsAsync(new V1Service()
                {
                     Spec = new V1ServiceSpec()
                     {
                         Selector = new Dictionary<string, string>()
                          {
                              { "app", "kaponata" },
                          },
                     },
                });

            var pod = new V1Pod();

            kubernetes.Setup(k => k.ListPodAsync(null, null, "app=kaponata", null, default))
                .ReturnsAsync(new V1PodList()
                {
                    Items = new List<V1Pod>() { pod },
                });

            var httpClient = new HttpClient();
            kubernetes
                .Setup(k => k.CreatePodHttpClient(pod, 5000))
                .Returns(httpClient);

            var configuration = new ImageRegistryClientConfiguration("registry", 5000);

            var factory = new ImageRegistryClientFactory(kubernetes.Object, configuration);

            var client = await factory.CreateAsync(default).ConfigureAwait(false);

            Assert.Same(httpClient, client.HttpClient);
        }

        /// <summary>
        /// <see cref="ImageRegistryClientFactory.CreateAsync(CancellationToken)"/> throws an exception when the service
        /// which hosts the registry cannot be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_OutsideCluster_ServiceMissing_Throws_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(k => k.RunningInCluster).Returns(false);

            var serviceClient = new Mock<NamespacedKubernetesClient<V1Service>>();
            kubernetes.Setup(k => k.GetClient<V1Service>()).Returns(serviceClient.Object);

            serviceClient
                .Setup(s => s.TryReadAsync("registry", default))
                .ReturnsAsync((V1Service)null);

            var configuration = new ImageRegistryClientConfiguration("registry", 5000);

            var factory = new ImageRegistryClientFactory(kubernetes.Object, configuration);

            await Assert.ThrowsAsync<ImageRegistryException>(() => factory.CreateAsync(default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClientFactory.CreateAsync(CancellationToken)"/> throws an exception when no pods
        /// could be found which implement the service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_OutsideCluster_NoPod_Throws_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(k => k.RunningInCluster).Returns(false);

            var serviceClient = new Mock<NamespacedKubernetesClient<V1Service>>();
            kubernetes.Setup(k => k.GetClient<V1Service>()).Returns(serviceClient.Object);

            serviceClient
                .Setup(s => s.TryReadAsync("registry", default))
                .ReturnsAsync(new V1Service()
                {
                    Spec = new V1ServiceSpec()
                    {
                        Selector = new Dictionary<string, string>()
                          {
                              { "app", "kaponata" },
                          },
                    },
                });

            kubernetes.Setup(k => k.ListPodAsync(null, null, "app=kaponata", null, default))
                .ReturnsAsync(new V1PodList()
                {
                    Items = new List<V1Pod>() { },
                });

            var configuration = new ImageRegistryClientConfiguration("registry", 5000);

            var factory = new ImageRegistryClientFactory(kubernetes.Object, configuration);

            await Assert.ThrowsAsync<ImageRegistryException>(() => factory.CreateAsync(default)).ConfigureAwait(false);
        }
    }
}
