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
using System.Collections.Generic;
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

        /// <summary>
        /// The <see cref="KubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync"/> method validates
        /// the arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InstallOrUpgradeCustomResourceDefinitionAsync_ValidatesArguments_Async()
        {
            var clientMock = new Mock<KubernetesClient>()
            {
                CallBase = true,
            };

            var client = clientMock.Object;

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    null,
                    TimeSpan.Zero,
                    default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ValidationException>(
                () => client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    new V1CustomResourceDefinition() { },
                    TimeSpan.Zero,
                    default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ValidationException>(
                () => client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    new V1CustomResourceDefinition()
                    {
                        Metadata = new V1ObjectMeta() { },
                    },
                    TimeSpan.Zero,
                    default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ValidationException>(
                () => client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    new V1CustomResourceDefinition()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = "test",
                        },
                    },
                    TimeSpan.Zero,
                    default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ValidationException>(
                () => client.InstallOrUpgradeCustomResourceDefinitionAsync(
                    new V1CustomResourceDefinition()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = "test",
                            Labels = new Dictionary<string, string>(),
                        },
                    },
                    TimeSpan.Zero,
                    default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="KubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync"/> method validates
        /// the arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InstallOrUpgradeCustomResourceDefinitionAsync_CrdNotInstalled_InstallsCrd_Async()
        {
            var clientMock = new Mock<KubernetesClient>()
            {
                CallBase = true,
            };

            var timeout = TimeSpan.Zero;
            var crd = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.1" },
                    },
                },
            };

            clientMock.Setup(c => c.TryReadCustomResourceDefinitionAsync("test", default)).ReturnsAsync((V1CustomResourceDefinition)null).Verifiable();
            clientMock.Setup(c => c.CreateCustomResourceDefinitionAsync(crd, default)).ReturnsAsync(crd).Verifiable();

            var client = clientMock.Object;

            await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                crd,
                timeout,
                default).ConfigureAwait(false);

            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync"/> method installs
        /// the updated CRD if the installed CRD has no version attribute.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InstallOrUpgradeCustomResourceDefinitionAsync_CrdHasNoVersionAttribute_DeleteAndInstallsCrd_Async()
        {
            var clientMock = new Mock<KubernetesClient>()
            {
                CallBase = true,
            };

            var timeout = TimeSpan.Zero;
            var crdToInstall = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.2" },
                    },
                },
            };

            var crdInstalled = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>(),
                },
            };

            clientMock.Setup(c => c.TryReadCustomResourceDefinitionAsync("test", default)).ReturnsAsync(crdInstalled).Verifiable();
            clientMock.Setup(c => c.DeleteCustomResourceDefinitionAsync(crdInstalled, timeout, default)).Returns(Task.CompletedTask).Verifiable();
            clientMock.Setup(c => c.CreateCustomResourceDefinitionAsync(crdToInstall, default)).ReturnsAsync(crdToInstall).Verifiable();

            var client = clientMock.Object;

            await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                crdToInstall,
                timeout,
                default).ConfigureAwait(false);

            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync"/> method installs the CRD if the
        /// installed CRD is outdated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InstallOrUpgradeCustomResourceDefinitionAsync_CrdOutdated_DeleteAndInstallsCrd_Async()
        {
            var clientMock = new Mock<KubernetesClient>()
            {
                CallBase = true,
            };

            var timeout = TimeSpan.Zero;
            var crdToInstall = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.2" },
                    },
                },
            };

            var crdInstalled = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.1" },
                    },
                },
            };

            clientMock.Setup(c => c.TryReadCustomResourceDefinitionAsync("test", default)).ReturnsAsync(crdInstalled).Verifiable();
            clientMock.Setup(c => c.DeleteCustomResourceDefinitionAsync(crdInstalled, timeout, default)).Returns(Task.CompletedTask).Verifiable();
            clientMock.Setup(c => c.CreateCustomResourceDefinitionAsync(crdToInstall, default)).ReturnsAsync(crdToInstall).Verifiable();

            var client = clientMock.Object;

            await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                crdToInstall,
                timeout,
                default).ConfigureAwait(false);

            clientMock.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync"/> method does nothing if the
        /// installed CRD is up to date.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InstallOrUpgradeCustomResourceDefinitionAsync_CrdUpToDate_NoOp_Async()
        {
            var clientMock = new Mock<KubernetesClient>()
            {
                CallBase = true,
            };

            var timeout = TimeSpan.Zero;
            var crdToInstall = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.2" },
                    },
                },
            };

            var crdInstalled = new V1CustomResourceDefinition()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    Labels = new Dictionary<string, string>()
                    {
                        { Annotations.Version, "0.2" },
                    },
                },
            };

            clientMock.Setup(c => c.TryReadCustomResourceDefinitionAsync("test", default)).ReturnsAsync(crdInstalled).Verifiable();

            var client = clientMock.Object;

            await client.InstallOrUpgradeCustomResourceDefinitionAsync(
                crdToInstall,
                timeout,
                default).ConfigureAwait(false);

            clientMock.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.WaitForCustomResourceDefinitionEstablishedAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> immediately returns when the CRD is established.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_CrdAlreadyEstablished_Returns_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await client.WaitForCustomResourceDefinitionEstablishedAsync(
                    new V1CustomResourceDefinition()
                    {
                        Status = new V1CustomResourceDefinitionStatus()
                        {
                            Conditions = new V1CustomResourceDefinitionCondition[]
                            {
                                new V1CustomResourceDefinitionCondition()
                                {
                                     Type = "Established",
                                     Status = "True",
                                },
                            },
                        },
                    },
                    TimeSpan.FromMinutes(1),
                    default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> fails when the CRD is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_PodDeleted_Returns_Async()
        {
            var crd =
                new V1CustomResourceDefinition()
                {
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
                .Setup(p => p.WatchCustomResourceDefinitionAsync(crd, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((crd, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForCustomResourceDefinitionEstablishedAsync(crd, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback throws an exception when a deleted event was received.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => callback(WatchEventType.Deleted, crd)).ConfigureAwait(false);
                watchTask.SetException(ex);
                Assert.Equal("The CRD 'my-crd' was deleted.", ex.Message);

                // The watch task propagates this exception.
                await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> completes when the CRD is established.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_CrdEstablished_Returns_Async()
        {
            var crd =
                new V1CustomResourceDefinition()
                {
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
                .Setup(p => p.WatchCustomResourceDefinitionAsync(crd, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForCustomResourceDefinitionEstablishedAsync(crd, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate a first callback where nothing changes. The task keeps listening.
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, crd).ConfigureAwait(false));

                // Another condition is present, and True
                crd.Status = new V1CustomResourceDefinitionStatus()
                {
                    Conditions = new V1CustomResourceDefinitionCondition[]
                    {
                        new V1CustomResourceDefinitionCondition()
                        {
                            Type = "Terminating",
                            Status = "True",
                        },
                    },
                };
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, crd).ConfigureAwait(false));

                // The established condition is present, but false.
                crd.Status = new V1CustomResourceDefinitionStatus()
                {
                    Conditions = new V1CustomResourceDefinitionCondition[]
                    {
                        new V1CustomResourceDefinitionCondition()
                        {
                            Type = "Established",
                            Status = "False",
                        },
                    },
                };
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, crd).ConfigureAwait(false));

                // The callback signals to stop watching when the CRD is established.
                crd.Status = new V1CustomResourceDefinitionStatus()
                {
                    Conditions = new V1CustomResourceDefinitionCondition[]
                    {
                        new V1CustomResourceDefinitionCondition()
                        {
                            Type = "Established",
                            Status = "True",
                        },
                    },
                };
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Modified, crd).ConfigureAwait(false));

                // The watch completes successfully.
                watchTask.SetResult(WatchExitReason.ClientDisconnected);
                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_ApiDisconnects_Errors_Async()
        {
            var crd =
                new V1CustomResourceDefinition()
                {
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
                .Setup(p => p.WatchCustomResourceDefinitionAsync(crd, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForCustomResourceDefinitionEstablishedAsync(crd, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate the watch task stopping
                watchTask.SetResult(WatchExitReason.ServerDisconnected);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The API server unexpectedly closed the connection while watching CRD 'my-crd'.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WaitForCustomResourceDefinitionEstablishedAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WaitForCustomResourceDefinitionEstablishedAsync_RespectsTimeout_Async()
        {
            var crd =
                new V1CustomResourceDefinition()
                {
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
                .Setup(p => p.WatchCustomResourceDefinitionAsync(crd, It.IsAny<Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>>(), It.IsAny<CancellationToken>()))
                .Returns<V1CustomResourceDefinition, Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>>, CancellationToken>((pod, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WaitForCustomResourceDefinitionEstablishedAsync(crd, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The CRD 'my-crd' was not established within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }
    }
}
