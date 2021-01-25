// <copyright file="KubernetesClientTests.MobileDevice.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="MobileDevice"/>-related functionality in the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// <see cref="KubernetesClient.CreateMobileDeviceAsync(MobileDevice, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateMobileDeviceAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("value", () => client.CreateMobileDeviceAsync(null, default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.CreateMobileDeviceAsync(
                        new MobileDevice(),
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.CreateMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta(),
                        },
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.CreateMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                Name = "test",
                            },
                        },
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.CreateMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                NamespaceProperty = "test",
                            },
                        },
                        default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.CreateMobileDeviceAsync(MobileDevice, CancellationToken)"/> returns a <see cref="MobileDevice"/> object
        /// when the operation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateMobileDeviceAsync_Success_ReturnsObject_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            var mobileDevice = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                },
            };

            protocol.Setup(p => p.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                mobileDevice,
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* dryRun */,
                null /* fieldManager */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .Returns<object, string, string, string, string, string, string, string, Dictionary<string, List<string>>, CancellationToken>(
                    (value, group, version, namespaceParameter, plural, dryDrun, fieldManager, pretty, customHeaders, cancellationToken) =>
                    {
                        return Task.FromResult(
                            new HttpOperationResponse<object>()
                            {
                                Response = new HttpResponseMessage()
                                {
                                    Content = new StringContent(JsonConvert.SerializeObject(Assert.IsType<MobileDevice>(mobileDevice))),
                                    StatusCode = HttpStatusCode.OK,
                                },
                            });
                    });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var result = await client.CreateMobileDeviceAsync(mobileDevice, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.CreateMobileDeviceAsync(MobileDevice, CancellationToken)"/> throws an exception
        /// when the result JSON cannot be parsed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateMobileDeviceAsync_InvalidJson_ThrowsException_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            var mobileDevice = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                },
            };

            protocol.Setup(p => p.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                mobileDevice,
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* dryRun */,
                null /* fieldManager */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .Returns<object, string, string, string, string, string, string, string, Dictionary<string, List<string>>, CancellationToken>(
                    (value, group, version, namespaceParameter, plural, dryDrun, fieldManager, pretty, customHeaders, cancellationToken) =>
                    {
                        return Task.FromResult(
                            new HttpOperationResponse<object>()
                            {
                                Response = new HttpResponseMessage()
                                {
                                    Content = new StringContent("blah"),
                                    StatusCode = HttpStatusCode.OK,
                                },
                            });
                    });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<SerializationException>(() => client.CreateMobileDeviceAsync(mobileDevice, default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.ListMobileDeviceAsync(string, string, string, string, int?, CancellationToken)"/> returns a typed object
        /// when the operation completes successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListMobileDeviceAsync_Success_ReturnsTypedResponse_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* continueParameter */,
                null /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* timeoutSeconds */,
                null /* watch */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .ReturnsAsync(
                new HttpOperationResponse<object>()
                {
                    Response = new HttpResponseMessage()
                    {
                        Content =
                            new StringContent(
                                JsonConvert.SerializeObject(
                                    new MobileDeviceList()
                                    {
                                        Items = new MobileDevice[]
                                        {
                                            new MobileDevice()
                                            {
                                                Metadata = new V1ObjectMeta()
                                                {
                                                    Name = "device1",
                                                },
                                            },
                                            new MobileDevice()
                                            {
                                                Metadata = new V1ObjectMeta()
                                                {
                                                    Name = "device2",
                                                },
                                            },
                                        },
                                    })),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var list = await client.ListMobileDeviceAsync("default").ConfigureAwait(false);
                Assert.Collection(
                    list.Items,
                    d => { Assert.Equal("device1", d.Metadata.Name); },
                    d => { Assert.Equal("device2", d.Metadata.Name); });
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.ListMobileDeviceAsync(string, string, string, string, int?, CancellationToken)"/> throws a <see cref="JsonException"/>
        /// when invalid data is returned by the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListMobileDeviceAsync_InvalidJson_ThrowsException_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* continueParameter */,
                null /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* timeoutSeconds */,
                null /* watch */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .ReturnsAsync(
                new HttpOperationResponse<object>()
                {
                    Response = new HttpResponseMessage()
                    {
                        Content = new StringContent("blah"),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<SerializationException>(() => client.ListMobileDeviceAsync("default")).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadMobileDeviceAsync(string, string, CancellationToken)"/> returns <see langword="null"/>
        /// if the requested device does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadMobileDeviceAsync_DeviceNotFound_ReturnsNull_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* continueParameter */,
                "metadata.name=my-device" /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* timeoutSeconds */,
                null /* watch */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .ReturnsAsync(
                new HttpOperationResponse<object>()
                {
                    Response = new HttpResponseMessage()
                    {
                        Content = new StringContent("{ \"Items\": [] }"),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Null(await client.TryReadMobileDeviceAsync("default", "my-device", default).ConfigureAwait(false));
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadMobileDeviceAsync(string, string, CancellationToken)"/> returns <see langword="null"/>
        /// if the requested device does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadMobileDeviceAsync_DeviceFound_ReturnsObject_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* continueParameter */,
                "metadata.name=my-device" /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* timeoutSeconds */,
                null /* watch */,
                null /* pretty */,
                null /* customHeaders */,
                default /* cancellationToken */))
                .ReturnsAsync(
                new HttpOperationResponse<object>()
                {
                    Response = new HttpResponseMessage()
                    {
                        Content = new StringContent("{ \"Items\": [ { \"Metadata\": { \"Name\": \"my-device\" } } ] }"),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var device = await client.TryReadMobileDeviceAsync("default", "my-device", default).ConfigureAwait(false);
                Assert.Equal("my-device", device.Metadata.Name);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WatchMobileDeviceAsync"/> delegates its work to <see cref="IKubernetesProtocol.WatchNamespacedObjectAsync{TObject, TList}"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchMobileDeviceAsync_Works_Async()
        {
            MobileDevice device = new MobileDevice()
            {
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "default",
                    Name = "my-device",
                },
            };

            TaskCompletionSource<WatchExitReason> tcs = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(
                p => p.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                    device,
                    It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>>(),
                    It.IsAny<Func<WatchEventType, MobileDevice, Task<WatchResult>>>(),
                    default))
                .Returns(tcs.Task);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WatchMobileDeviceAsync(
                    device,
                    (eventType, device) =>
                    {
                        return Task.FromResult(WatchResult.Stop);
                    },
                    default);

                Assert.False(task.IsCompleted);
                tcs.SetResult(WatchExitReason.ClientDisconnected);

                Assert.Equal(WatchExitReason.ClientDisconnected, await task);
            }

            protocol.Verify();
        }

        [Fact]
        public async Task DeleteMobileDeviceAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteMobileDeviceAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => client.DeleteMobileDeviceAsync(new MobileDevice() { }, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.DeleteMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta() { },
                        },
                        TimeSpan.FromMinutes(1),
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.DeleteMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                Name = "test",
                            },
                        },
                        TimeSpan.FromMinutes(1),
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => client.DeleteMobileDeviceAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                NamespaceProperty = "test",
                            },
                        },
                        TimeSpan.FromMinutes(1),
                        default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteMobileDeviceAsync"/> returns when the mobile device is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteMobileDeviceAsync_DeviceDeleted_Returns_Async()
        {
            var mobileDevice =
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                };

            Func<WatchEventType, MobileDevice, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose()).Verifiable();

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>>(),
                        It.IsAny<Func<WatchEventType, MobileDevice, Task<WatchResult>>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>, Func<WatchEventType, MobileDevice, Task<WatchResult>>, CancellationToken>(
                (device, list, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            protocol
                .Setup(
                    p => p.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                        "kaponata.io",
                        "v1alpha1",
                        "default",
                        "mobiledevices",
                        "my-device",
                        null, /* body */
                        null, /* gracePeriodSeconds */
                        null, /* orphanDependents */
                        null, /* propagationPolicy */
                        null, /* dryRun */
                        null, /* customHeaders */
                        default))
                .Returns(Task.FromResult(new HttpOperationResponse<object>() { Body = mobileDevice, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteMobileDeviceAsync(mobileDevice, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback continues watching until the pod is deleted
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, mobileDevice).ConfigureAwait(false));
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Deleted, mobileDevice).ConfigureAwait(false));
                watchTask.SetResult(WatchExitReason.ClientDisconnected);

                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteMobileDeviceAsync"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteMobileDeviceAsync_ApiDisconnects_Errors_Async()
        {
            var mobileDevice =
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                };

            Func<WatchEventType, MobileDevice, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>>(),
                        It.IsAny<Func<WatchEventType, MobileDevice, Task<WatchResult>>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>, Func<WatchEventType, MobileDevice, Task<WatchResult>>, CancellationToken>(
                (device, list, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            protocol
                .Setup(
                    p => p.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                        "kaponata.io",
                        "v1alpha1",
                        "default",
                        "mobiledevices",
                        "my-device",
                        null, /* body */
                        null, /* gracePeriodSeconds */
                        null, /* orphanDependents */
                        null, /* propagationPolicy */
                        null, /* dryRun */
                        null, /* customHeaders */
                        default))
                .Returns(Task.FromResult(new HttpOperationResponse<object>() { Body = mobileDevice, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteMobileDeviceAsync(mobileDevice, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // Simulate the watch task stopping
                watchTask.SetResult(WatchExitReason.ServerDisconnected);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The API server unexpectedly closed the connection while watching MobileDevice 'my-device'.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteMobileDeviceAsync"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteMobileDeviceAsync_RespectsTimeout_Async()
        {
            var mobileDevice =
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-device",
                        NamespaceProperty = "default",
                    },
                };

            Func<WatchEventType, MobileDevice, Task<WatchResult>> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>>(),
                        It.IsAny<Func<WatchEventType, MobileDevice, Task<WatchResult>>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, MobileDeviceList>, Func<WatchEventType, MobileDevice, Task<WatchResult>>, CancellationToken>(
                (device, list, watcher, ct) =>
                {
                    callback = watcher;
                    return watchTask.Task;
                });

            protocol
                .Setup(
                    p => p.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                        "kaponata.io",
                        "v1alpha1",
                        "default",
                        "mobiledevices",
                        "my-device",
                        null, /* body */
                        null, /* gracePeriodSeconds */
                        null, /* orphanDependents */
                        null, /* propagationPolicy */
                        null, /* dryRun */
                        null, /* customHeaders */
                        default))
                .Returns(Task.FromResult(new HttpOperationResponse<object>() { Body = mobileDevice, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteMobileDeviceAsync(mobileDevice, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The MobileDevice 'my-device' was not deleted within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }
    }
}
