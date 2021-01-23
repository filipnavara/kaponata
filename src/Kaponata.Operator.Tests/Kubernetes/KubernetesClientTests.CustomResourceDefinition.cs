// <copyright file="KubernetesClientTests.CustomResourceDefinition.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the CRD-specific methods in the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// The <see cref="KubernetesClient.CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition, CancellationToken)"/> method validates the arguments passed to it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateCustomResourceDefinitionAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.CreateCustomResourceDefinitionAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The <see cref="KubernetesClient.CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition, CancellationToken)"/> method calles into <see cref="IKubernetesProtocol"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateCustomResourceDefinitionAsync_UsesProtocol_Async()
        {
            var crd = new V1CustomResourceDefinition() { Metadata = new V1ObjectMeta() { Name = "my-crd" } };
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.CreateCustomResourceDefinitionWithHttpMessagesAsync(crd, null, null, null, null, default))
                .ReturnsAsync(new HttpOperationResponse<V1CustomResourceDefinition>() { Body = crd });
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var result = await client.CreateCustomResourceDefinitionAsync(crd, default).ConfigureAwait(false);
                Assert.Same(crd, result);
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadCustomResourceDefinitionAsync(string, CancellationToken)"/> returns the object if the pod exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadCustomResourceDefinitionAsync_Found_ReturnsPod_Async()
        {
            var crd = new V1CustomResourceDefinition();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.ListCustomResourceDefinitionWithHttpMessagesAsync(null, null, "metadata.name=my-crd", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(new HttpOperationResponse<V1CustomResourceDefinitionList>() { Body = new V1CustomResourceDefinitionList() { Items = new V1CustomResourceDefinition[] { crd } } });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Equal(crd, await client.TryReadCustomResourceDefinitionAsync("my-crd", default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadPodAsync(string, string, CancellationToken)"/> returns the <see langword="null"/> if the pod does exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadCustomResourceDefinitionAsync_NotFound_ReturnsNull_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.ListCustomResourceDefinitionWithHttpMessagesAsync(null, null, "metadata.name=my-crd", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(new HttpOperationResponse<V1CustomResourceDefinitionList>() { Body = new V1CustomResourceDefinitionList() { Items = new V1CustomResourceDefinition[] { } } });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Null(await client.TryReadCustomResourceDefinitionAsync("my-crd", default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteCustomResourceDefinitionAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCustomResourceDefinitionAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.DeleteCustomResourceDefinitionAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteCustomResourceDefinitionAsync"/> returns when the CustomResourceDefinition is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCustomResourceDefinitionAsync_CustomResourceDefinitionDeleted_Returns_Async()
        {
            var customResourceDefinition =
                new V1CustomResourceDefinition()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-crd",
                    },
                    Status = new V1CustomResourceDefinitionStatus()
                    {
                    },
                };

            Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteCustomResourceDefinitionWithHttpMessagesAsync(customResourceDefinition.Metadata.Name, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Status>() { Body = new V1Status(), Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchCustomResourceDefinitionAsync(customResourceDefinition, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((customResourceDefinition, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteCustomResourceDefinitionAsync(customResourceDefinition, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback continues watching until the CustomResourceDefinition is deleted
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, customResourceDefinition).ConfigureAwait(false));
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Deleted, customResourceDefinition).ConfigureAwait(false));
                watchTask.SetResult(WatchExitReason.ClientDisconnected);

                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteCustomResourceDefinitionAsync"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCustomResourceDefinitionAsync_ApiDisconnects_Errors_Async()
        {
            var customResourceDefinition =
                new V1CustomResourceDefinition()
                {
                    Kind = V1CustomResourceDefinition.KubeKind,
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-crd",
                    },
                };

            Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteCustomResourceDefinitionWithHttpMessagesAsync(customResourceDefinition.Metadata.Name, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Status>() { Body = new V1Status(), Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchCustomResourceDefinitionAsync(customResourceDefinition, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((customResourceDefinition, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteCustomResourceDefinitionAsync(customResourceDefinition, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate the watch task stopping
                watchTask.SetResult(WatchExitReason.ServerDisconnected);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The API server unexpectedly closed the connection while watching CustomResourceDefinition 'my-crd'.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteCustomResourceDefinitionAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCustomResourceDefinitionAsync_RespectsTimeout_Async()
        {
            var customResourceDefinition =
                new V1CustomResourceDefinition()
                {
                    Kind = V1CustomResourceDefinition.KubeKind,
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-crd",
                    },
                    Status = new V1CustomResourceDefinitionStatus()
                    {
                    },
                };

            Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteCustomResourceDefinitionWithHttpMessagesAsync(customResourceDefinition.Metadata.Name, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Status>() { Body = new V1Status(), Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchCustomResourceDefinitionAsync(customResourceDefinition, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((customResourceDefinition, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteCustomResourceDefinitionAsync(customResourceDefinition, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The CustomResourceDefinition 'my-crd' was not deleted within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }
    }
}
