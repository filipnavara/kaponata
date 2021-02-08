// <copyright file="KubernetesClientTests.Pods.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the pod-specific methods in the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// <see cref="KubernetesClient.CreatePodHttpClient(V1Pod, int)"/> validates the arguments passed to it.
        /// </summary>
        [Fact]
        public void CreatePodHttpClient_ValidatesArguments()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Throws<ArgumentNullException>("pod", () => client.CreatePodHttpClient(null, 80));
                Assert.Throws<ValidationException>(() => client.CreatePodHttpClient(new V1Pod(), 80));
                Assert.Throws<ValidationException>(() => client.CreatePodHttpClient(new V1Pod() { Metadata = new V1ObjectMeta() }, 80));
                Assert.Throws<ValidationException>(() => client.CreatePodHttpClient(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "foo" } }, 80));
                Assert.Throws<ValidationException>(() => client.CreatePodHttpClient(new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "bar" } }, 80));
            }
        }

        /// <summary>
        /// The <see cref="HttpClient"/> returned by <see cref="KubernetesClient.CreatePodHttpClient(V1Pod, int)"/> is
        /// configured correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreatePodHttpClient_IsConfiguredCorrectly_Async()
        {
            var pod = new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default", Name = "my-pod" } };

            var mock = new Mock<KubernetesClient>(MockBehavior.Strict);
            mock.Setup(c => c.CreatePodHttpClient(pod, 8080)).CallBase();
            mock.Setup(c => c.ConnectToPodPortAsync(pod, 8080, It.IsAny<CancellationToken>())).ReturnsAsync(Stream.Null).Verifiable();
            mock.Setup(c => c.Dispose());

            using (var client = mock.Object)
            using (var httpClient = client.CreatePodHttpClient(pod, 8080))
            {
                Assert.NotNull(httpClient);
                Assert.Equal(new Uri("http://my-pod:8080"), httpClient.BaseAddress);

                // Executing the request will result in an attempt to read a HTTP response off Stream.Null, which
                // will throw an IOException wrapped in a HttpRequestException.
                // Verify the exception and make sure ConnectToPodPortAsync was called as basic verification.
                var exception = await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.GetStreamAsync("/")).ConfigureAwait(false);
                Assert.IsType<IOException>(exception.InnerException);
                mock.Verify();
            }
        }

        /// <summary>
        /// The <see cref="HttpClient"/> returned by <see cref="KubernetesClient.CreatePodHttpClient(V1Pod, int)"/> throws
        /// when trying to connect to an endpoint which is no a different host or a different port.
        /// </summary>
        /// <param name="url">
        /// The URL to use in the request.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("http://another-pod:8080/")]
        [InlineData("http://my-pod:81/")]
        public async Task CreatePodHttpClient_OtherHost_Throws_Async(string url)
        {
            var pod = new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default", Name = "my-pod" } };

            var mock = new Mock<KubernetesClient>(MockBehavior.Strict);
            mock.Setup(c => c.CreatePodHttpClient(pod, 8080)).CallBase();
            mock.Setup(c => c.Dispose());

            using (var client = mock.Object)
            using (var httpClient = client.CreatePodHttpClient(pod, 8080))
            {
                Assert.NotNull(httpClient);
                Assert.Equal(new Uri("http://my-pod:8080"), httpClient.BaseAddress);

                // Executing the request will result in the connect callback failing validation,
                // which will throw an InvalidOperationException wrapped in a HttpRequestException.
                // Verify the exception and make sure ConnectToPodPortAsync was called as basic verification.
                var exception = await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.GetStreamAsync(url)).ConfigureAwait(false);
                Assert.IsType<InvalidOperationException>(exception.InnerException);
                mock.Verify();
            }
        }
    }
}
