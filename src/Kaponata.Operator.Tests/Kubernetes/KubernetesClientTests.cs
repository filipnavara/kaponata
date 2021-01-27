// <copyright file="KubernetesClientTests.cs" company="Quamotion bv">
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
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// The <see cref="KubernetesClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("protocol", () => new KubernetesClient(null, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesClient(Mock.Of<IKubernetesProtocol>(), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesClient(Mock.Of<IKubernetesProtocol>(), NullLogger<KubernetesClient>.Instance, null));
        }

        /// <summary>
        /// Calling <see cref="KubernetesClient.Dispose"/> also disposes of the protocol.
        /// </summary>
        [Fact]
        public void Dispose_DisposesProtocol()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance);
            client.Dispose();

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.WaitForPodRunningAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> immediately returns when the pod is running.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_PodRunning_Returns_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await client.WaitForPodRunningAsync(
                    new V1Pod()
                    {
                        Status = new V1PodStatus()
                        {
                            Phase = "Running",
                        },
                    },
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> immediately fails when the pod has failed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_PodFailed_Returns_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => client.WaitForPodRunningAsync(
                    new V1Pod()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = "my-pod",
                        },
                        Status = new V1PodStatus()
                        {
                            Phase = "Failed",
                            Reason = "Something went wrong!",
                        },
                    },
                    TimeSpan.FromMinutes(1),
                    default)).ConfigureAwait(false);

                Assert.Equal("The pod my-pod has failed: Something went wrong!", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> fails when the pod is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_PodDeleted_Returns_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback throws an exception when a deleted event was received.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => callback(WatchEventType.Deleted, pod)).ConfigureAwait(false);
                watchTask.SetException(ex);
                Assert.Equal("The pod my-pod was deleted.", ex.Message);

                // The watch task propagates this exception.
                await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> fails when the pod fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_PodFails_Returns_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback throws an exception when a deleted event was received.
                pod.Status.Phase = "Failed";
                pod.Status.Reason = "Something went wrong!";

                var ex = await Assert.ThrowsAsync<KubernetesException>(() => callback(WatchEventType.Modified, pod)).ConfigureAwait(false);
                watchTask.SetException(ex);
                Assert.Equal("The pod my-pod has failed: Something went wrong!", ex.Message);

                // The watch task propagates this exception.
                await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> completes when the pod starts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_PodStarts_Returns_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate a first callback where nothing changes. The task keeps listening.
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, pod).ConfigureAwait(false));

                // The callback signals to stop watching when the pod starts.
                pod.Status.Phase = "Running";
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Modified, pod).ConfigureAwait(false));

                // The watch completes successfully.
                watchTask.SetResult(WatchExitReason.ClientDisconnected);
                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_ApiDisconnects_Errors_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForPodRunningAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate the watch task stopping
                watchTask.SetResult(WatchExitReason.ServerDisconnected);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The API server unexpectedly closed the connection while watching pod my-pod.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForPodRunningAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForPodRunning_RespectsTimeout_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForPodRunningAsync(pod, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The pod my-pod did not transition to the completed state within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeletePodAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePodAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.DeletePodAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeletePodAsync"/> returns when the pod is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePodAsync_PodDeleted_Returns_Async()
        {
            var pod =
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteNamespacedPodWithHttpMessagesAsync(pod.Metadata.Name, pod.Metadata.NamespaceProperty, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Pod>() { Body = pod, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeletePodAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback continues watching until the pod is deleted
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, pod).ConfigureAwait(false));
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Deleted, pod).ConfigureAwait(false));
                watchTask.SetResult(WatchExitReason.ClientDisconnected);

                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeletePodAsync"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePodAsync_ApiDisconnects_Errors_Async()
        {
            var pod =
                new V1Pod()
                {
                    Kind = V1Pod.KubeKind,
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteNamespacedPodWithHttpMessagesAsync(pod.Metadata.Name, pod.Metadata.NamespaceProperty, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Pod>() { Body = pod, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeletePodAsync(pod, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate the watch task stopping
                watchTask.SetResult(WatchExitReason.ServerDisconnected);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The API server unexpectedly closed the connection while watching Pod 'my-pod'.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeletePodAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePodAsync_RespectsTimeout_Async()
        {
            var pod =
                new V1Pod()
                {
                    Kind = V1Pod.KubeKind,
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            WatchEventDelegate<V1Pod> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.DeleteNamespacedPodWithHttpMessagesAsync(pod.Metadata.Name, pod.Metadata.NamespaceProperty, null, null, null, null, null, null, null, default))
                .Returns(Task.FromResult(new HttpOperationResponse<V1Pod>() { Body = pod, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            protocol
                .Setup(p => p.WatchNamespacedObjectAsync(pod, protocol.Object.ListNamespacedPodWithHttpMessagesAsync, It.IsAny<WatchEventDelegate<V1Pod>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, V1PodList>, WatchEventDelegate<V1Pod>, CancellationToken>((pod, list, watcher, ct) =>
                {
                    Assert.NotNull(list);
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeletePodAsync(pod, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The Pod 'my-pod' was not deleted within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesClient.CreatePodAsync(V1Pod, CancellationToken)"/> method validates the arguments passed to it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreatePodAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.CreatePodAsync(null, default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.CreatePodAsync(new V1Pod(), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.CreatePodAsync(new V1Pod() { Metadata = new V1ObjectMeta() }, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The <see cref="KubernetesClient.CreatePodAsync(V1Pod, CancellationToken)"/> method captures any <see cref="V1Status"/> error messages
        /// embedded in Kubernetes responses.
        /// </summary>
        /// <param name="statusCode">
        /// The status code returned by the server.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.UnprocessableEntity)]
        public async Task CreatePodAsync_CapturesKubernetesError_Async(HttpStatusCode statusCode)
        {
            const string status = @"{""kind"":""Status"",""apiVersion"":""v1"",""metadata"":{},""status"":""Failure"",""message"":""pods 'waitforpodrunning-integrationtest-async' already exists"",""reason"":""AlreadyExists"",""details"":{ ""name"":""waitforpodrunning-integrationtest-async"",""kind"":""pods""},""code"":409}";

            var pod = new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default" } };

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.CreateNamespacedPodWithHttpMessagesAsync(pod, pod.Metadata.NamespaceProperty, null, null, null, null, default))
                .ThrowsAsync(
                    new HttpOperationException()
                    {
                        Response = new HttpResponseMessageWrapper(
                            new HttpResponseMessage(statusCode),
                            status),
                    });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => client.CreatePodAsync(pod, default)).ConfigureAwait(false);
                Assert.Equal("pods 'waitforpodrunning-integrationtest-async' already exists", ex.Message);
                Assert.NotNull(ex.Status);
            }
        }

        /// <summary>
        /// The <see cref="KubernetesClient.CreatePodAsync(V1Pod, CancellationToken)"/> method throws the originale exception
        /// when the V1Status object could not be parsed.
        /// </summary>
        /// <param name="statusJson">
        /// The status data by the server.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null)] // null string
        [InlineData("")] // Empty string
        [InlineData("{}")] // Empty json
        [InlineData(@"{""kind"":""Pod"", ""apiVersion"":""v1"" }")] // Unexpected object kind
        [InlineData(@"{""kind"":""Status"", ""apiVersion"":""v1beta1"", ""message"":""pods 'waitforpodrunning-integrationtest-async' already exists""}")] // Unexpected object version
        [InlineData(@"{""kind"":""Status"", ""apiVersion"":""v1beta1"" }")] // No message
        [InlineData(@"{""kind"":""Status"", ""apiVersion"":""v1beta1"", ""message"":""""}")] // Empty message
        public async Task CreatePodAsync_InvalidKubernetesError_Async(string statusJson)
        {
            var pod = new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "default" } };

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.CreateNamespacedPodWithHttpMessagesAsync(pod, pod.Metadata.NamespaceProperty, null, null, null, null, default))
                .ThrowsAsync(
                    new HttpOperationException()
                    {
                        Response = new HttpResponseMessageWrapper(
                            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity),
                            statusJson),
                    });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<HttpOperationException>(() => client.CreatePodAsync(pod, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadPodAsync(string, string, CancellationToken)"/> returns the object if the pod exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadPod_Found_ReturnsPod_Async()
        {
            var pod = new V1Pod();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.ListNamespacedPodWithHttpMessagesAsync("default", null, null, "metadata.name=my-pod", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(new HttpOperationResponse<V1PodList>() { Body = new V1PodList() { Items = new V1Pod[] { pod } } });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Equal(pod, await client.TryReadPodAsync("default", "my-pod", default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadPodAsync(string, string, CancellationToken)"/> returns <see langword="null"/> if the pod does exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadPod_NotFound_ReturnsNull_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.ListNamespacedPodWithHttpMessagesAsync("default", null, null, "metadata.name=my-pod", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(new HttpOperationResponse<V1PodList>() { Body = new V1PodList() { Items = new V1Pod[] { } } });

            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Null(await client.TryReadPodAsync("default", "my-pod", default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="KubernetesClient.ConnectToPodPortAsync(V1Pod, int, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ConnectToPodPortAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.ConnectToPodPortAsync(null, 1, default).AsTask()).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.ConnectToPodPortAsync(new V1Pod { }, 1, default).AsTask()).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.ConnectToPodPortAsync(new V1Pod { Metadata = new V1ObjectMeta() }, 1, default).AsTask()).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.ConnectToPodPortAsync(new V1Pod { Metadata = new V1ObjectMeta() { Name = "a" } }, 1, default).AsTask()).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.ConnectToPodPortAsync(new V1Pod { Metadata = new V1ObjectMeta() { NamespaceProperty = "b" } }, 1, default).AsTask()).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.ConnectToPodPortAsync"/> uses Kubernetes port forwarding.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ConnectToPodPortAsync_UsesPortForwarding_Async()
        {
            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "my-namespace",
                    Name = "usbmuxd-abcd",
                },
            };

            var websocket = Mock.Of<WebSocket>();
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(k => k.WebSocketNamespacedPodPortForwardAsync("usbmuxd-abcd", "my-namespace", new int[] { 27015 }, null, null, default))
                .ReturnsAsync(websocket)
                .Verifiable();
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            using (var stream = await client.ConnectToPodPortAsync(pod, 27015, default))
            {
                var portForwardStream = Assert.IsType<PortForwardStream>(stream);
                Assert.Same(websocket, portForwardStream.WebSocket);
            }

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesClient.ListPodAsync"/> method forwards requests to <see cref="IKubernetesProtocol"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListPodAsync_ForwardRequests_Async()
        {
            var list = new V1PodList();
            var response = new HttpOperationResponse<V1PodList>() { Body = list };
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.ListNamespacedPodWithHttpMessagesAsync("namespace", null, "continue", "fieldSelector", "labelSelector", 1, null, null, null, null, null, null, default))
                .ReturnsAsync(response);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Same(list, await client.ListPodAsync("namespace", "continue", "fieldSelector", "labelSelector", 1, default).ConfigureAwait(false));
            }

            protocol.Verify();
        }

        /// <summary>
        /// KubernetesClient.WatchPodAsync forwards requests to <see cref="IKubernetesProtocol"/>.
        /// </summary>
        [Fact]
        public void WatchPodAsync_ForwardRequests()
        {
            var tcs = new TaskCompletionSource<WatchExitReason>();
            var eventHandler = new WatchEventDelegate<V1Pod>((type, pod) => Task.FromResult(WatchResult.Continue));

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync("namespace", "fieldSelector", "labelSelector", "resourceVersion", protocol.Object.ListNamespacedPodWithHttpMessagesAsync, eventHandler, default))
                .Returns(tcs.Task);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Same(tcs.Task, client.WatchPodAsync("namespace", "fieldSelector", "labelSelector", "resourceVersion", eventHandler, default));
            }

            protocol.Verify();
        }
    }
}
