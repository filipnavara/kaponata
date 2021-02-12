// <copyright file="RedroidOperatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Kaponata.Operator.Operators;
using Kaponata.Tests.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="RedroidOperator"/> class.
    /// </summary>
    public class RedroidOperatorTests
    {
        /// <summary>
        /// The <see cref="RedroidOperator"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new RedroidOperator(null, NullLogger<RedroidOperator>.Instance));
            Assert.Throws<ArgumentNullException>(() => new RedroidOperator(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop does nothing if the cluster is empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EmptyCluster_NoOp_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>();
            kubernetes.Setup(c => c.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                Assert.True(await @operator.ReconcileAsync(default).ConfigureAwait(false));
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop does nothing if the cluster is empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PodAndDevice_NoOp_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android",
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "redroid",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[]
                        {
                            new V1ContainerStatus()
                            {
                                Name = "redroid",
                                Ready = true,
                            },
                        },
                    },
                });

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator",
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        NamespaceProperty = "default",
                        Name = "redroid",
                    },
                });

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                Assert.True(await @operator.ReconcileAsync(default).ConfigureAwait(false));
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop does not create a new device if a new Redroid pod
        /// is detected which is not running.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_Pending_DoesNotCreateDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android",
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Pending",
                    },
                });

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                Assert.True(await @operator.ReconcileAsync(default).ConfigureAwait(false));
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop does not create a new device if a new Redroid pod
        /// is detected which is running but not ready.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_NotReady_DoesNotCreateDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android",
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[]
                        {
                            new V1ContainerStatus()
                            {
                                Name = "redroid",
                                Ready = false,
                            },
                        },
                    },
                });

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                Assert.True(await @operator.ReconcileAsync(default).ConfigureAwait(false));
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop creates a new device if a new Redroid pod
        /// is detected which is running and for which all containers are ready.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_CreatesDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android",
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[] { },
                    },
                });

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                Assert.True(await @operator.ReconcileAsync(default).ConfigureAwait(false));
            }

            Assert.Collection(
                createdDevices,
                d =>
                {
                    Assert.Equal("default", d.Metadata.NamespaceProperty);
                    Assert.Equal("my-device", d.Metadata.Name);
                    Assert.Equal("RedroidOperator", d.Metadata.Labels[Annotations.ManagedBy]);
                    Assert.Equal("android", d.Metadata.Labels[Annotations.Os]);
                });
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ReconcileAsync"/> loop deletes devices for which no matching Redroid
        /// pod is detected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeletedPod_DeletesDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            var device = new MobileDevice()
            {
                Metadata = new V1ObjectMeta(),
            };

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator",
                device);

            (var createdDevices, _) = deviceClient.TrackCreatedItems();
            var deletedDevices = deviceClient.TrackDeletedItems();

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                await @operator.ReconcileAsync(default).ConfigureAwait(false);
            }

            Assert.Empty(createdDevices);
            Assert.Collection(
                deletedDevices,
                d => Assert.Equal(device, d));
        }

        /// <summary>
        /// The <see cref="RedroidOperator.ExecuteAsync(CancellationToken)"/> method listens for the cancellation requests and completes
        /// timely.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task RedroidOperator_CompletedWhenWatchCompletes_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Empty cluster, so the Reconcile loop does nothing.
            var podWatchCts = kubernetes.WithPodWatcher(
                labelSelector: "kubernetes.io/os=android");
            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            var deviceWatchCts = deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                await @operator.StartAsync(default).ConfigureAwait(false);
                await Task.WhenAll(podWatchCts.ClientRegistered.Task, deviceWatchCts.ClientRegistered.Task).ConfigureAwait(false);
                var task = @operator.StopAsync(default).ConfigureAwait(false);
            }

            kubernetes.Verify();
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> executes the reconciliation loop when a pod event is received.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RedroidOperator_ReconcilesOnPodEvent_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Empty cluster, so the Reconcile loop at first nothing but start the watcher
            var podWatchCts = kubernetes.WithPodWatcher(
                labelSelector: "kubernetes.io/os=android");
            var pods = kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            var deviceWatchCts = deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                await @operator.StartAsync(default).ConfigureAwait(false);
                await Task.WhenAll(podWatchCts.ClientRegistered.Task, deviceWatchCts.ClientRegistered.Task).ConfigureAwait(false);

                var newPod = new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "redroid",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[]
                        {
                            new V1ContainerStatus()
                            {
                                Name = "redroid",
                                Ready = true,
                            },
                        },
                    },
                };

                pods.Add(newPod);
                (var newDeviceList, _) = deviceClient.TrackCreatedItems();
                await (await podWatchCts.ClientRegistered.Task.ConfigureAwait(false))(WatchEventType.Added, newPod).ConfigureAwait(false);

                await @operator.StopAsync(default).ConfigureAwait(false);
                Assert.Single(newDeviceList);
            }
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> executes the reconciliation loop when a device event is received.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RedroidOperator_ReconcilesOnDeviceEvent_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Empty cluster, so the Reconcile loop at first nothing but start the watcher
            var podWatchCts = kubernetes.WithPodWatcher(
                labelSelector: "kubernetes.io/os=android");
            var pods = kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            var deviceWatchCts = deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                await @operator.StartAsync(default).ConfigureAwait(false);
                await Task.WhenAll(podWatchCts.ClientRegistered.Task, deviceWatchCts.ClientRegistered.Task).ConfigureAwait(false);

                var newPod = new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "redroid",
                        NamespaceProperty = "default",
                    },
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[]
                        {
                            new V1ContainerStatus()
                            {
                                Name = "redroid",
                                Ready = true,
                            },
                        },
                    },
                };

                pods.Add(newPod);
                (var newDeviceList, _) = deviceClient.TrackCreatedItems();
                await (await deviceWatchCts.ClientRegistered.Task.ConfigureAwait(false))(WatchEventType.Added, new MobileDevice()).ConfigureAwait(false);

                await @operator.StopAsync(default).ConfigureAwait(false);
                Assert.Single(newDeviceList);
            }
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> stops executing when one of the watchers stop executing.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        /// <remarks>
        /// This is the current behavior; we may well want to change this so that the watchers automatically restart.
        /// </remarks>
        [Fact]
        public async Task RedroidOperator_Stops_WhenWatchersStop_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Empty cluster, so the Reconcile loop at first nothing but start the watcher
            var podWatchCts = kubernetes.WithPodWatcher(
                labelSelector: "kubernetes.io/os=android");
            kubernetes.WithPodList(
                labelSelector: "kubernetes.io/os=android");

            var deviceClient = kubernetes.WithClient<MobileDevice>();
            var deviceWatchCts = deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");
            deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android,app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = new RedroidOperator(
                kubernetes.Object,
                NullLogger<RedroidOperator>.Instance))
            {
                await @operator.StartAsync(default).ConfigureAwait(false);

                Assert.True(@operator.IsRunning);

                await Task.WhenAll(podWatchCts.ClientRegistered.Task, deviceWatchCts.ClientRegistered.Task).ConfigureAwait(false);

                // Simulate the pod watcher and device watcher quitting
                Assert.True(@operator.IsRunning);

                podWatchCts.TaskCompletionSource.SetResult(WatchExitReason.ServerDisconnected);
                deviceWatchCts.TaskCompletionSource.SetResult(WatchExitReason.ServerDisconnected);

                await @operator.WaitForCompletion.ConfigureAwait(false);

                Assert.False(@operator.IsRunning);

                // operator.StopAsync will throw an exception because the operator is not running.
                var ex = await Assert.ThrowsAsync<AggregateException>(() => @operator.StopAsync(default)).ConfigureAwait(false);
                Assert.IsType<InvalidOperationException>(ex.GetBaseException().InnerException);

                Assert.False(@operator.IsRunning);
            }
        }
    }
}
