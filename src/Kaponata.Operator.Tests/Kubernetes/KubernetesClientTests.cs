// <copyright file="KubernetesClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="KubernetesClient"/> class.
    /// </summary>
    public class KubernetesClientTests
    {
        /// <summary>
        /// The <see cref="KubernetesClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("protocol", () => new KubernetesClient(null, NullLogger<KubernetesClient>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesClient(Mock.Of<IKubernetesProtocol>(), null));
        }

        /// <summary>
        /// Calling <see cref="KubernetesClient.Dispose"/> also disposes of the protocol.
        /// </summary>
        [Fact]
        public void Dispose_DisposesProtocol()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance);
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

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("pod", () => client.WaitForPodRunningAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
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

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
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
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-pod",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                };

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
            {
                var task = client.DeletePodAsync(pod, TimeSpan.FromMinutes(1), default);
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
        /// <see cref="KubernetesClient.DeletePodAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletePodAsync_RespectsTimeout_Async()
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

            Func<WatchEventType, V1Pod, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol
                .Setup(p => p.WatchPodAsync(pod, It.IsAny<Func<WatchEventType, V1Pod, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1Pod, Func<WatchEventType, V1Pod, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance))
            {
                var task = client.DeletePodAsync(pod, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The pod my-pod was not deleted within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }
    }
}
