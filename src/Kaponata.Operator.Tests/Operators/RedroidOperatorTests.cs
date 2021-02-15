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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="RedroidOperator"/> class.
    /// </summary>
    public class RedroidOperatorTests
    {
        private readonly Mock<KubernetesClient> kubernetes;
        private readonly Mock<NamespacedKubernetesClient<V1Pod>> podClient;
        private readonly Mock<NamespacedKubernetesClient<MobileDevice>> deviceClient;
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedroidOperatorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// An output helper to use when logging.
        /// </param>
        public RedroidOperatorTests(ITestOutputHelper output)
        {
            this.kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            this.podClient = this.kubernetes.WithClient<V1Pod>();
            this.deviceClient = this.kubernetes.WithClient<MobileDevice>();

            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddSingleton(this.kubernetes.Object);
                    services.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output);
                        });
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// <see cref="RedroidOperator.BuildRedroidOperator(IServiceProvider)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void BuildRedroidOperator_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => RedroidOperator.BuildRedroidOperator(null));
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> loop does nothing if the cluster is empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EmptyCluster_NoOp_Async()
        {
            this.podClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android");

            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = this.deviceClient.TrackCreatedItems();
            var deletedDevices = this.deviceClient.TrackDeletedItems();

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.InitializeAsync(default);
                Assert.Equal(0, @operator.ReconcilationBuffer.Count);
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> loop does nothing if a matching pod and device object
        /// exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PodAndDevice_NoOp_Async()
        {
            var pod = new V1Pod()
            {
                Kind = V1Pod.KubeKind,
                ApiVersion = V1Pod.KubeApiVersion,
                Metadata = new V1ObjectMeta()
                {
                    Name = "redroid",
                    NamespaceProperty = "default",
                    Uid = "uid",
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

            this.podClient.WithList(
            fieldSelector: null,
            labelSelector: "kubernetes.io/os=android",
            pod);

            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator",
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        NamespaceProperty = "default",
                        Name = "redroid",
                        OwnerReferences = new V1OwnerReference[] { pod.AsOwnerReference() },
                    },
                });

            (var createdDevices, _) = this.deviceClient.TrackCreatedItems();
            var deletedDevices = this.deviceClient.TrackDeletedItems();

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);

                // The parent-child pair will be scheduled for reconciliation, but the reconcilation itself
                // will not do anything.
                Assert.Equal(1, @operator.ReconcilationBuffer.Count);
                @operator.ReconcilationBuffer.Post(null);
                await @operator.ProcessBufferedReconciliationsAsync(default).ConfigureAwait(false);
                Assert.Equal(0, @operator.ReconcilationBuffer.Count);
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> loop does not create a new device if a new Redroid pod
        /// is detected which is not running.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_Pending_DoesNotCreateDevice_Async()
        {
            this.podClient.WithList(
                fieldSelector: null,
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

            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = this.deviceClient.TrackCreatedItems();
            var deletedDevices = this.deviceClient.TrackDeletedItems();

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);
                Assert.Equal(0, @operator.ReconcilationBuffer.Count);
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> loop does not create a new device if a new Redroid pod
        /// is detected which is running but not ready.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_NotReady_DoesNotCreateDevice_Async()
        {
            this.podClient.WithList(
                fieldSelector: null,
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

            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            (var createdDevices, _) = this.deviceClient.TrackCreatedItems();
            var deletedDevices = this.deviceClient.TrackDeletedItems();

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.InitializeAsync(default);
                Assert.Equal(0, @operator.ReconcilationBuffer.Count);
            }

            Assert.Empty(createdDevices);
            Assert.Empty(deletedDevices);
        }

        /// <summary>
        /// The <see cref="RedroidOperator"/> creates a new device if a new Redroid pod
        /// is detected which is running and for which all containers are ready.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task NewPod_CreatesDevice_Async()
        {
            this.podClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android",
                new V1Pod()
                {
                    ApiVersion = V1Pod.KubeApiVersion,
                    Kind = V1Pod.KubeKind,
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

            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            this.deviceClient
                .Setup(d => d.TryReadAsync("my-device", "app.kubernetes.io/managed-by=RedroidOperator", It.IsAny<CancellationToken>()))
                .ReturnsAsync((MobileDevice)null);

            (var createdDevices, _) = this.deviceClient.TrackCreatedItems();
            var deletedDevices = this.deviceClient.TrackDeletedItems();

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);
                Assert.Equal(1, @operator.ReconcilationBuffer.Count);
                @operator.ReconcilationBuffer.Post(null);
                await @operator.ProcessBufferedReconciliationsAsync(default);
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
        /// The <see cref="RedroidOperator"/> executes the reconciliation loop when a pod event is received.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RedroidOperator_ReconcilesOnPodEvent_Async()
        {
            // Empty cluster, so the Reconcile loop at first nothing but start the watcher
            var podWatchCts = this.podClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android");
            var pods = this.podClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android");

            var deviceWatchCts = this.deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");
            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
            {
                await @operator.StartAsync(default).ConfigureAwait(false);
                await Task.WhenAll(podWatchCts.ClientRegistered.Task, deviceWatchCts.ClientRegistered.Task).ConfigureAwait(false);

                var newPod = new V1Pod()
                {
                    ApiVersion = V1Pod.KubeApiVersion,
                    Kind = V1Pod.KubeKind,
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
                this.podClient.Setup(d => d.TryReadAsync("redroid", "kubernetes.io/os=android", It.IsAny<CancellationToken>())).ReturnsAsync(newPod);
                this.deviceClient.Setup(d => d.TryReadAsync("redroid", "app.kubernetes.io/managed-by=RedroidOperator", It.IsAny<CancellationToken>())).ReturnsAsync((MobileDevice)null);

                (var newDeviceList, _) = this.deviceClient.TrackCreatedItems();
                var podWatcher = await podWatchCts.ClientRegistered.Task.ConfigureAwait(false);
                await podWatcher(WatchEventType.Added, newPod).ConfigureAwait(false);

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
            // Empty cluster, so the Reconcile loop at first nothing but start the watcher
            var podWatchCts = this.podClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android");
            this.podClient.WithList(
                fieldSelector: null,
                labelSelector: "kubernetes.io/os=android");

            var deviceWatchCts = this.deviceClient.WithWatcher(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");
            this.deviceClient.WithList(
                fieldSelector: null,
                labelSelector: "app.kubernetes.io/managed-by=RedroidOperator");

            using (var @operator = RedroidOperator.BuildRedroidOperator(this.host.Services).Build())
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
