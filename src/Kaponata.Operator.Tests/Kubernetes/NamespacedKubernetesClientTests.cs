// <copyright file="NamespacedKubernetesClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="NamespacedKubernetesClient{T}"/> class.
    /// </summary>
    public class NamespacedKubernetesClientTests
    {
        /// <summary>
        /// The <see cref="NamespacedKubernetesClient{T}"/> constructor validates the arguments passed to it.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("parent", () => new NamespacedKubernetesClient<V1Pod>(null, new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "core")));
            Assert.Throws<ArgumentNullException>("metadata", () => new NamespacedKubernetesClient<V1Pod>(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> validates the arguments passed to it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_ValidatesArguments_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata);

            await Assert.ThrowsAsync<ArgumentNullException>("namespace", () => client.TryDeleteAsync(null, "test", TimeSpan.Zero, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("name", () => client.TryDeleteAsync("test", null, TimeSpan.Zero, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> returns <see langword="null"/> if the requested object does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_DoesNotExist_DoesNothing_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Return an empty list when searching for the requested pod.
            parent
                .Setup(l => l.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, "default", null, null, "metadata.name=my-name", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(
                    new HttpOperationResponse<ItemList<V1Pod>>()
                    {
                        Body = new ItemList<V1Pod>()
                        {
                            Items = new V1Pod[] { },
                        },
                    });

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata);
            Assert.Null(await client.TryDeleteAsync("default", "my-name", TimeSpan.FromMinutes(1), default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> deletes the object and returns the deleted object if the requested
        /// object exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_DoesExist_DoesDelete_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Return the requested pod when searching for the requested pod.
            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            parent
                .Setup(l => l.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, "default", null, null, "metadata.name=my-name", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(
                    new HttpOperationResponse<ItemList<V1Pod>>()
                    {
                        Body = new ItemList<V1Pod>()
                        {
                            Items = new V1Pod[]
                            {
                                pod,
                            },
                        },
                    });

            parent
                .Setup(l => l.DeleteNamespacedObjectAsync<V1Pod>(
                    pod,
                    It.IsAny<V1DeleteOptions>(),
                    It.IsAny<KubernetesClient.DeleteNamespacedObjectAsyncDelegate<V1Pod>>(),
                    It.IsAny<KubernetesClient.WatchObjectAsyncDelegate<V1Pod>>(),
                    TimeSpan.FromMinutes(1),
                    default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata);
            Assert.Equal(pod, await client.TryDeleteAsync("default", "my-name", TimeSpan.FromMinutes(1), default).ConfigureAwait(false));

            parent.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.PatchAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PatchAsync_ValidatesParameters_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);
            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata);

            await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.PatchAsync(null, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchAsync(new V1Pod(), new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchAsync(new V1Pod() { Metadata = new V1ObjectMeta() }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchAsync(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "foo" } }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchAsync(new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "bar" } }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("patch", () => client.PatchAsync(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "foo", NamespaceProperty = "bar" } }, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="NamespacedKubernetesClient{T}.PatchStatusAsync"/> method correctly invokes
        /// the underlying object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PatchAsync_Works_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose());

            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            protocol
                .Setup(
                    p => p.PatchNamespacedCustomObjectWithHttpMessagesAsync(
                        It.IsAny<V1Patch>(),
                        string.Empty,
                        "v1",
                        "default",
                        "pods",
                        "my-pod",
                        null,
                        null,
                        null,
                        null,
                        default))
                .ReturnsAsync(
                    new HttpOperationResponse<object>()
                    {
                        Response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(pod)),
                        },
                    });

            using (var parent = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var client = new NamespacedKubernetesClient<V1Pod>(parent, metadata);

                var patch = new JsonPatchDocument<V1Pod>();
                patch.Replace(d => d.Spec.Containers, new V1Container[] { new V1Container() { Image = "my-image", }, });

                var result = await client.PatchAsync(pod, patch, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.PatchStatusAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PatchStatusAsync_ValidatesParameters_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);
            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata);

            await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.PatchStatusAsync(null, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchStatusAsync(new V1Pod(), new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchStatusAsync(new V1Pod() { Metadata = new V1ObjectMeta() }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchStatusAsync(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "foo" } }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => client.PatchStatusAsync(new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "bar" } }, new JsonPatchDocument<V1Pod>(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("patch", () => client.PatchStatusAsync(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "foo", NamespaceProperty = "bar" } }, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="NamespacedKubernetesClient{T}.PatchStatusAsync"/> method correctly invokes
        /// the underlying object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PatchStatusAsync_Works_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose());

            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            protocol
                .Setup(
                    p => p.PatchNamespacedCustomObjectStatusWithHttpMessagesAsync(
                        It.IsAny<V1Patch>(),
                        string.Empty,
                        "v1",
                        "default",
                        "pods",
                        "my-pod",
                        null,
                        null,
                        null,
                        null,
                        default))
                .ReturnsAsync(
                    new HttpOperationResponse<object>()
                    {
                        Response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(pod)),
                        },
                    });

            using (var parent = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var client = new NamespacedKubernetesClient<V1Pod>(parent, metadata);

                var patch = new JsonPatchDocument<V1Pod>();
                patch.Replace(d => d.Spec.Containers, new V1Container[] { new V1Container() { Image = "my-image", }, });

                var result = await client.PatchStatusAsync(pod, patch, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }
    }
}
