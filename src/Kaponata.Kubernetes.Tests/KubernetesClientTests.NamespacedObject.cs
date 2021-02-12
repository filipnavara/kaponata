// <copyright file="KubernetesClientTests.NamespacedObject.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Microsoft.Rest;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the methods which operate on namespaced objects in the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// <see cref="KubernetesClient.CreateNamespacedValueAsync{T}(KindMetadata, T, CancellationToken)"/> validates the arguments
        /// passed to it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateNamespacedValueAsync_ValidatesArguments_Async()
        {
            var mock = new Mock<KubernetesClient>(MockBehavior.Strict);
            mock.Setup(c => c.Dispose());
            mock.Setup(c => c.CreateNamespacedValueAsync<V1Pod>(It.IsAny<KindMetadata>(), It.IsAny<V1Pod>(), default))
                .CallBase();

            using (var client = mock.Object)
            {
                var kind = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, "pods");

                await Assert.ThrowsAsync<ArgumentNullException>("kind", () => client.CreateNamespacedValueAsync(null, new V1Pod(), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.CreateNamespacedValueAsync(kind, (V1Pod)null, default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.CreateNamespacedValueAsync(kind, new V1Pod(), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.CreateNamespacedValueAsync(
                    kind,
                    new V1Pod()
                    {
                        Metadata = new V1ObjectMeta(),
                    },
                    default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.CreateNamespacedValueAsync(
                    kind,
                    new V1Pod()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            NamespaceProperty = "invalid",
                        },
                    },
                    default)).ConfigureAwait(false);
            }
        }
    }
}
