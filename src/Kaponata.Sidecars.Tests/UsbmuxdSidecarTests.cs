// <copyright file="UsbmuxdSidecarTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Sidecars.Tests
{
    /// <summary>
    /// Tests the <see cref="UsbmuxdSidecar"/> class.
    /// </summary>
    public class UsbmuxdSidecarTests
    {
        /// <summary>
        /// The <see cref="UsbmuxdSidecar"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("muxerClient", () => new UsbmuxdSidecar(null, Mock.Of<KubernetesClient>(), Mock.Of<PairingRecordProvisioner>(), new UsbmuxdSidecarConfiguration(), NullLogger<UsbmuxdSidecar>.Instance));
            Assert.Throws<ArgumentNullException>("kubernetes", () => new UsbmuxdSidecar(Mock.Of<MuxerClient>(), null, Mock.Of<PairingRecordProvisioner>(), new UsbmuxdSidecarConfiguration(), NullLogger<UsbmuxdSidecar>.Instance));
            Assert.Throws<ArgumentNullException>("pairingRecordProvisioner", () => new UsbmuxdSidecar(Mock.Of<MuxerClient>(), Mock.Of<KubernetesClient>(), null, new UsbmuxdSidecarConfiguration(), NullLogger<UsbmuxdSidecar>.Instance));
            Assert.Throws<ArgumentNullException>("configuration", () => new UsbmuxdSidecar(Mock.Of<MuxerClient>(), Mock.Of<KubernetesClient>(), Mock.Of<PairingRecordProvisioner>(), null, NullLogger<UsbmuxdSidecar>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new UsbmuxdSidecar(Mock.Of<MuxerClient>(), Mock.Of<KubernetesClient>(), Mock.Of<PairingRecordProvisioner>(), new UsbmuxdSidecarConfiguration(), null));
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> throws when the <see cref="UsbmuxdSidecarConfiguration.PodName"/>
        /// is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_InvalidPod_Throws_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>();
            podClient.Setup(p => p.TryReadAsync("my-pod", default)).Returns(Task.FromResult((V1Pod)null));
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            using (var sidecar = new UsbmuxdSidecar(Mock.Of<MuxerClient>(), kubernetes.Object, Mock.Of<PairingRecordProvisioner>(), configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => sidecar.ReconcileAsync(default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> does nothing when the muxer and Kubernetes device list
        /// are both empty.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_EmptyLists_DoesNothing_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ReturnsAsync(new V1Pod());
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, "kubernetes.io/os=ios,app.kubernetes.io/managed-by=UsbmuxdSidecar,app.kubernetes.io/instance=my-pod", null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>(),
                    })
                .Verifiable();
            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>())
                .Verifiable();

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, Mock.Of<PairingRecordProvisioner>(), configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await sidecar.ReconcileAsync(default).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> creates a new device when required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_NewDevice_CreatesDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            var pod = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ReturnsAsync(pod);
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, "kubernetes.io/os=ios,app.kubernetes.io/managed-by=UsbmuxdSidecar,app.kubernetes.io/instance=my-pod", null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>(),
                    })
                .Verifiable();
            deviceClient
                .Setup(d => d.CreateAsync(It.IsAny<MobileDevice>(), default))
                .Returns<MobileDevice, CancellationToken>((device, ct) =>
                {
                    Assert.Collection(
                        device.Metadata.Labels,
                        l =>
                        {
                            Assert.Equal(Annotations.Os, l.Key);
                            Assert.Equal(Annotations.OperatingSystem.iOS, l.Value);
                        },
                        l =>
                        {
                            Assert.Equal(Annotations.ManagedBy, l.Key);
                            Assert.Equal("UsbmuxdSidecar", l.Value);
                        },
                        l =>
                        {
                            Assert.Equal(Annotations.Instance, l.Key);
                            Assert.Equal("my-pod", l.Value);
                        });

                    Assert.Equal("my-udid", device.Metadata.Name);
                    Assert.Equal("default", device.Metadata.NamespaceProperty);

                    var ownerReference = Assert.Single(device.Metadata.OwnerReferences);
                    Assert.Equal("my-pod", ownerReference.Name);

                    return Task.FromResult(device);
                })
                .Verifiable();

            deviceClient
                .Setup(d => d.PatchStatusAsync(It.IsAny<MobileDevice>(), It.IsAny<JsonPatchDocument<MobileDevice>>(), default))
                .Returns<MobileDevice, JsonPatchDocument<MobileDevice>, CancellationToken>(
                (d, patch, ct) =>
                {
                    Assert.Equal(ConditionStatus.True, d.Status.GetConditionStatus(MobileDeviceConditions.Paired));
                    return Task.FromResult(d);
                });

            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(
                    new Collection<MuxerDevice>()
                    {
                        new MuxerDevice()
                        {
                            Udid = "my-udid",
                        },
                    })
                .Verifiable();
            muxer
                .Setup(m => m.ReadPairingRecordAsync("my-udid", default))
                .ReturnsAsync((PairingRecord)null);

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            var provisioner = new Mock<PairingRecordProvisioner>();
            provisioner.Setup(p => p.ProvisionPairingRecordAsync("my-udid", default)).ReturnsAsync(new PairingRecord()).Verifiable();

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, provisioner.Object, configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await sidecar.ReconcileAsync(default).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> does not create a new Kubernetes device when the device
        /// did not pair with the host.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_NewDevice_NotPaired_DoesNothing_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            var pod = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ReturnsAsync(pod);
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, "kubernetes.io/os=ios,app.kubernetes.io/managed-by=UsbmuxdSidecar,app.kubernetes.io/instance=my-pod", null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>(),
                    })
                .Verifiable();

            deviceClient
                .Setup(d => d.CreateAsync(It.IsAny<MobileDevice>(), default))
                .Returns<MobileDevice, CancellationToken>((device, ct) =>
                {
                    return Task.FromResult(device);
                })
                .Verifiable();

            deviceClient
                .Setup(d => d.PatchStatusAsync(It.IsAny<MobileDevice>(), It.IsAny<JsonPatchDocument<MobileDevice>>(), default))
                .Returns<MobileDevice, JsonPatchDocument<MobileDevice>, CancellationToken>(
                (d, patch, ct) =>
                {
                    Assert.Equal(ConditionStatus.False, d.Status.GetConditionStatus(MobileDeviceConditions.Paired));
                    return Task.FromResult(d);
                });

            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(
                    new Collection<MuxerDevice>()
                    {
                        new MuxerDevice()
                        {
                            Udid = "my-udid",
                        },
                    })
                .Verifiable();

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            var provisioner = new Mock<PairingRecordProvisioner>();
            provisioner.Setup(p => p.ProvisionPairingRecordAsync("my-udid", default)).ReturnsAsync((PairingRecord)null).Verifiable();

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, provisioner.Object, configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await sidecar.ReconcileAsync(default).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> deletes outdated devices.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_OutdatedDevice_RemovesDevice_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            var pod = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ReturnsAsync(pod);
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var outdatedDevice = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-udid",
                },
            };

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, "kubernetes.io/os=ios,app.kubernetes.io/managed-by=UsbmuxdSidecar,app.kubernetes.io/instance=my-pod", null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>() { outdatedDevice },
                    })
                .Verifiable();
            deviceClient
                .Setup(d => d.DeleteAsync(outdatedDevice, It.IsAny<TimeSpan>(), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(new Collection<MuxerDevice>())
                .Verifiable();

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, Mock.Of<PairingRecordProvisioner>(), configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await sidecar.ReconcileAsync(default).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> does nothing when the muxer and Kubernetes
        /// device lists match.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_MatchingDevice_DoesNothing_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            var pod = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ReturnsAsync(pod);
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var kubernetesDevice = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-udid",
                },
                Status = new MobileDeviceStatus()
                {
                    Conditions = new List<MobileDeviceCondition>()
                    {
                        new MobileDeviceCondition()
                        {
                            Type = MobileDeviceConditions.Paired,
                            Status = ConditionStatus.True,
                            Reason = "Trusted",
                            Message = "The device has trusted the host.",
                            LastHeartbeatTime = DateTimeOffset.Now,
                        },
                    },
                },
            };

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, "kubernetes.io/os=ios,app.kubernetes.io/managed-by=UsbmuxdSidecar,app.kubernetes.io/instance=my-pod", null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>() { kubernetesDevice },
                    })
                .Verifiable();

            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            muxer
                .Setup(m => m.ListDevicesAsync(default))
                .ReturnsAsync(
                    new Collection<MuxerDevice>()
                    {
                        new MuxerDevice()
                        {
                            Udid = "my-udid",
                        },
                    })
                .Verifiable();
            muxer
                .Setup(m => m.ReadPairingRecordAsync("my-udid", default))
                .ReturnsAsync((PairingRecord)null);

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            var provisioner = new Mock<PairingRecordProvisioner>();
            provisioner.Setup(p => p.ProvisionPairingRecordAsync("my-udid", default)).ReturnsAsync(new PairingRecord()).Verifiable();

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, provisioner.Object, configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await sidecar.ReconcileAsync(default).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
            provisioner.Verify();
        }

        /// <summary>
        /// <see cref="UsbmuxdSidecar.ReconcileAsync(CancellationToken)"/> throws when any of the underlying
        /// calls throws.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Reconcile_ThrowsOnError_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var podClient = new Mock<NamespacedKubernetesClient<V1Pod>>(MockBehavior.Strict);
            var pod = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-pod",
                    NamespaceProperty = "default",
                },
            };

            podClient.Setup(p => p.TryReadAsync("my-pod", default)).ThrowsAsync(new HttpOperationException());
            kubernetes.Setup(k => k.GetClient<V1Pod>()).Returns(podClient.Object);

            var kubernetesDevice = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-udid",
                },
            };

            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            kubernetes.Setup(k => k.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);

            var configuration = new UsbmuxdSidecarConfiguration()
            {
                PodName = "my-pod",
            };

            using (var sidecar = new UsbmuxdSidecar(muxer.Object, kubernetes.Object, Mock.Of<PairingRecordProvisioner>(), configuration, NullLogger<UsbmuxdSidecar>.Instance))
            {
                await Assert.ThrowsAsync<HttpOperationException>(() => sidecar.ReconcileAsync(default)).ConfigureAwait(false);
            }

            deviceClient.Verify();
            muxer.Verify();
        }

        /// <summary>
        /// The <see cref="UsbmuxdSidecar"/> response to <see cref="MuxerClient.ListenAsync"/> callbacks
        /// by scheduling a reconciliation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ExecuteAsync_HandlesListenEvents_Async()
        {
            var muxer = new Mock<MuxerClient>(MockBehavior.Strict);
            var listenCompleted = new TaskCompletionSource<bool>();

            Func<DeviceAttachedMessage, CancellationToken, Task<MuxerListenAction>> attachedCallback = null;
            Func<DeviceDetachedMessage, CancellationToken, Task<MuxerListenAction>> detachedCallback = null;
            Func<DevicePairedMessage, CancellationToken, Task<MuxerListenAction>> pairedCallback = null;

            var listenInitialized = new TaskCompletionSource();

            muxer.Setup(
                m => m.ListenAsync(
                    It.IsAny<Func<DeviceAttachedMessage, CancellationToken, Task<MuxerListenAction>>>(),
                    It.IsAny<Func<DeviceDetachedMessage, CancellationToken, Task<MuxerListenAction>>>(),
                    It.IsAny<Func<DevicePairedMessage, CancellationToken, Task<MuxerListenAction>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Func<DeviceAttachedMessage, CancellationToken, Task<MuxerListenAction>>, Func<DeviceDetachedMessage, CancellationToken, Task<MuxerListenAction>>, Func<DevicePairedMessage, CancellationToken, Task<MuxerListenAction>>, CancellationToken>(
                    (attached, detached, paired, ct) =>
                    {
                        attachedCallback = attached;
                        detachedCallback = detached;
                        pairedCallback = paired;
                        listenInitialized.SetResult();
                    })
                .Returns(listenCompleted.Task);

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes
                .Setup(k => k.GetClient<MobileDevice>())
                .Returns(new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict).Object);

            var configuration = new UsbmuxdSidecarConfiguration();

            var mre = new AutoResetEvent(false);

            var sidecarMock = new Mock<UsbmuxdSidecar>(muxer.Object, kubernetes.Object, Mock.Of<PairingRecordProvisioner>(), configuration, NullLogger<UsbmuxdSidecar>.Instance);
            sidecarMock.CallBase = true;
            sidecarMock
                .Setup(s => s.ReconcileAsync(It.IsAny<CancellationToken>()))
                .Callback(() => mre.Set())
                .Returns(Task.CompletedTask);

            using (var sidecar = sidecarMock.Object)
            {
                await sidecar.StartAsync(default);

                Assert.True(mre.WaitOne(TimeSpan.FromSeconds(5)), "Failed to schedule the initial reconciliation");

                await listenInitialized.Task.ConfigureAwait(false);

                Assert.Equal(MuxerListenAction.ContinueListening, await attachedCallback(new DeviceAttachedMessage() { Properties = new DeviceProperties() }, default).ConfigureAwait(false));
                Assert.True(mre.WaitOne(0), "Failed to schedule the reconciliation after a device attached message");

                Assert.Equal(MuxerListenAction.ContinueListening, await detachedCallback(new DeviceDetachedMessage(), default).ConfigureAwait(false));
                Assert.True(mre.WaitOne(0), "Failed to schedule the reconciliation after a device detached message");

                Assert.Equal(MuxerListenAction.ContinueListening, await pairedCallback(new DevicePairedMessage(), default).ConfigureAwait(false));
                Assert.True(mre.WaitOne(0), "Failed to schedule the reconciliation after a device paired message");

                listenCompleted.SetResult(true);
                await sidecar.StopAsync(default);
            }
        }
    }
}
