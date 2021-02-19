// <copyright file="NamespacedKubernetesClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.AspNetCore.JsonPatch;
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

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

namespace Kaponata.Kubernetes.Tests
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
            Assert.Throws<ArgumentNullException>("parent", () => new NamespacedKubernetesClient<V1Pod>(null, new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "core"), NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance));
            Assert.Throws<ArgumentNullException>("metadata", () => new NamespacedKubernetesClient<V1Pod>(Mock.Of<KubernetesClient>(), null, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new NamespacedKubernetesClient<V1Pod>(Mock.Of<KubernetesClient>(), new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "core"), null));
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.CreateAsync(T, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();

                await Assert.ThrowsAsync<ArgumentNullException>("value", () => mobileDeviceClient.CreateAsync(null, default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.CreateAsync(
                        new MobileDevice(),
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.CreateAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta(),
                        },
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.CreateAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta()
                            {
                                Name = "test",
                                NamespaceProperty = "my-namespace",
                            },
                        },
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.CreateAsync(
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
        /// <see cref="NamespacedKubernetesClient{T}.CreateAsync(T, CancellationToken)"/> returns a <see cref="MobileDevice"/> object
        /// when the operation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_Success_ReturnsObject_Async()
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
                                    Content = new StringContent(JsonConvert.SerializeObject(Assert.IsType<MobileDevice>(value))),
                                    StatusCode = HttpStatusCode.OK,
                                },
                            });
                    });

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                var result = await mobileDeviceClient.CreateAsync(mobileDevice, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.CreateAsync(T, CancellationToken)"/> throws an exception
        /// when the result JSON cannot be parsed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_InvalidJson_ThrowsException_Async()
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                await Assert.ThrowsAsync<SerializationException>(() => mobileDeviceClient.CreateAsync(mobileDevice, default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.ListAsync(string?, string?, string?, int?, CancellationToken)"/> returns a typed object
        /// when the operation completes successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_Success_ReturnsTypedResponse_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* allowWatchBookmarks */,
                null /* continueParameter */,
                null /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* resourceVersionMatch */,
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
                                    new ItemList<MobileDevice>()
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                var list = await mobileDeviceClient.ListAsync().ConfigureAwait(false);
                Assert.Collection(
                    list.Items,
                    d => { Assert.Equal("device1", d.Metadata.Name); },
                    d => { Assert.Equal("device2", d.Metadata.Name); });
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.ListAsync(string?, string?, string?, int?, CancellationToken)"/> throws a <see cref="JsonException"/>
        /// when invalid data is returned by the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_InvalidJson_ThrowsException_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "mobiledevices" /* plural */,
                null /* allowWatchBookmarks */,
                null /* continueParameter */,
                null /* fieldSelector */,
                null /* labelSelector */,
                null /* limit */,
                null /* resourceVersion */,
                null /* resourceVersionMatch */,
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                await Assert.ThrowsAsync<SerializationException>(() => mobileDeviceClient.ListAsync()).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> methods validates its arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task DeleteAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                await Assert.ThrowsAsync<ArgumentNullException>(() => mobileDeviceClient.DeleteAsync(null, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(() => mobileDeviceClient.DeleteAsync(new MobileDevice() { }, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.DeleteAsync(
                        new MobileDevice()
                        {
                            Metadata = new V1ObjectMeta() { },
                        },
                        TimeSpan.FromMinutes(1),
                        default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ValidationException>(
                    () => mobileDeviceClient.DeleteAsync(
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
                    () => mobileDeviceClient.DeleteAsync(
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
        /// <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> returns when the mobile device is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_DeviceDeleted_Returns_Async()
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

            WatchEventDelegate<MobileDevice> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose()).Verifiable();

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, ItemList<MobileDevice>>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>>(),
                        It.IsAny<WatchEventDelegate<MobileDevice>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>, WatchEventDelegate<MobileDevice>, CancellationToken>(
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                var task = mobileDeviceClient.DeleteAsync(mobileDevice, TimeSpan.FromMinutes(1), default);
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
        /// <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> throws an exception when an invalid
        /// response is received.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteMobileDeviceAsync_InvalidContent_Errors_Async()
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

            WatchEventDelegate<MobileDevice> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose()).Verifiable();

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, ItemList<MobileDevice>>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>>(),
                        It.IsAny<WatchEventDelegate<MobileDevice>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>, WatchEventDelegate<MobileDevice>, CancellationToken>(
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
                .Returns(Task.FromResult(new HttpOperationResponse<object>() { Body = mobileDevice, Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("--") } })).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                await Assert.ThrowsAsync<SerializationException>(() => mobileDeviceClient.DeleteAsync(mobileDevice, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> errors when the API server disconnects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ApiDisconnects_Errors_Async()
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

            WatchEventDelegate<MobileDevice> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, ItemList<MobileDevice>>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>>(),
                        It.IsAny<WatchEventDelegate<MobileDevice>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>, WatchEventDelegate<MobileDevice>, CancellationToken>(
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                var task = mobileDeviceClient.DeleteAsync(mobileDevice, TimeSpan.FromMinutes(1), default);
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
        /// <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> respects the time out passed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_RespectsTimeout_Async()
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

            WatchEventDelegate<MobileDevice> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<MobileDevice, ItemList<MobileDevice>>(
                        mobileDevice,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>>(),
                        It.IsAny<WatchEventDelegate<MobileDevice>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<MobileDevice, ListNamespacedObjectWithHttpMessagesAsync<MobileDevice, ItemList<MobileDevice>>, WatchEventDelegate<MobileDevice>, CancellationToken>(
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

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var mobileDeviceClient = client.GetClient<MobileDevice>();
                var task = mobileDeviceClient.DeleteAsync(mobileDevice, TimeSpan.Zero, default);
                Assert.NotNull(callback);

                // The watch completes with an exception.
                var ex = await Assert.ThrowsAsync<KubernetesException>(() => task).ConfigureAwait(false);
                Assert.Equal("The MobileDevice 'my-device' was not deleted within a timeout of 0 seconds.", ex.Message);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> validates the arguments passed to it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_ValidatesArguments_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>("name", () => client.TryDeleteAsync(null, TimeSpan.Zero, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> returns <see langword="null"/> if the requested object does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_DoesNotExist_DoesNothing_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);

            // Return an empty list when searching for the requested pod.
            parent
                .Setup(l => l.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, null, null, "metadata.name=my-name", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(
                    new HttpOperationResponse<ItemList<V1Pod>>()
                    {
                        Body = new ItemList<V1Pod>()
                        {
                            Items = new V1Pod[] { },
                        },
                    });

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            Assert.Null(await client.TryDeleteAsync("my-name", TimeSpan.FromMinutes(1), default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryDeleteAsync"/> deletes the object and returns the deleted object if the requested
        /// object exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryDeleteAsync_DoesExist_DoesDelete_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
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
                .Setup(l => l.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, null, null, "metadata.name=my-name", null, null, null, null, null, null, null, null, default))
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

            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            Assert.Equal(pod, await client.TryDeleteAsync("my-name", TimeSpan.FromMinutes(1), default).ConfigureAwait(false));

            parent.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.PatchAsync"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PatchAsync_ValidatesParameters_Async()
        {
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);
            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

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
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
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

            using (var parent = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var client = new NamespacedKubernetesClient<V1Pod>(parent, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

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
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
            var parent = new Mock<KubernetesClient>(MockBehavior.Strict);
            var client = new NamespacedKubernetesClient<V1Pod>(parent.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

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
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");
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

            using (var parent = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var client = new NamespacedKubernetesClient<V1Pod>(parent, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

                var patch = new JsonPatchDocument<V1Pod>();
                patch.Replace(d => d.Spec.Containers, new V1Container[] { new V1Container() { Image = "my-image", }, });

                var result = await client.PatchStatusAsync(pod, patch, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryReadAsync(string, string, CancellationToken)"/>  validates the arguments
        /// passed to it.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryReadAsync_ValidatesArguments_Async()
        {
            var client = new NamespacedKubernetesClient<V1Pod>(Mock.Of<KubernetesClient>(), new KindMetadata(string.Empty, string.Empty, string.Empty, string.Empty), NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>("name", () => client.TryReadAsync(null, "selector", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryReadAsync(string, string, CancellationToken)"/> returns the requested value
        /// if it exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadAsync_ItemExists_Works_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var metadata = new KindMetadata("core", V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            V1Pod pod = new V1Pod();
            var pods = new ItemList<V1Pod>()
            {
                Items = new V1Pod[]
                {
                    pod,
                },
            };

            kubernetes
                .Setup(k => k.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, null, null, "metadata.name=name", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(
                    new HttpOperationResponse<ItemList<V1Pod>>()
                    {
                        Body = pods,
                    });

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            var value = await client.TryReadAsync("name", null, default).ConfigureAwait(false);

            Assert.Same(pod, value);

            // Additionally, calling TryReadAsync also populates the "ApiGroup" and "Kind" properties on the
            // individual items. This acts as a workaround for https://github.com/kubernetes/kubernetes/issues/80609
            // https://github.com/kubernetes/kubernetes/issues/3030 and https://github.com/kubernetes/kubernetes/pull/80618
            Assert.Equal("core/v1", value.ApiVersion);
            Assert.Equal(V1Pod.KubeKind, value.Kind);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.TryReadAsync(string, string, CancellationToken)"/> returns <see langword="null"/>
        /// if the requested value does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadAsync_ItemDoesNotExist_Works_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            var pods = new ItemList<V1Pod>()
            {
                Items = new V1Pod[] { },
            };

            kubernetes
                .Setup(k => k.ListNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>(metadata, null, null, "metadata.name=name", null, null, null, null, null, null, null, null, default))
                .ReturnsAsync(
                    new HttpOperationResponse<ItemList<V1Pod>>()
                    {
                        Body = pods,
                    });

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            var value = await client.TryReadAsync("name", null, default).ConfigureAwait(false);

            Assert.Null(value);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.WatchAsync(string, string, string, WatchEventDelegate{T}, CancellationToken)"/>
        /// exits when <see cref="KubernetesClient.WatchNamespacedObjectAsync{TObject, TList}(string, string, string, ListNamespacedObjectWithHttpMessagesAsync{TObject, TList}, WatchEventDelegate{TObject}, CancellationToken)"/>
        /// returns <see cref="WatchExitReason.ClientDisconnected"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchAsync_ClientDisconnects_Returns_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes
                .Setup(w => w.WatchNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>("fieldSelector", "labelSelector", "resourceVersion", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>>(), It.IsAny<WatchEventDelegate<V1Pod>>(), default))
                .ReturnsAsync(WatchExitReason.ClientDisconnected)
                .Verifiable();
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            WatchEventDelegate<V1Pod> eventHandler = (eventType, value) => { return Task.FromResult(WatchResult.Stop); };

            // WatchAsync will exit when WatchNamespacedObjectAsync returns ClientDisconnected
            await client.WatchAsync("fieldSelector", "labelSelector", "resourceVersion", eventHandler, default).ConfigureAwait(false);

            kubernetes.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.WatchAsync(string, string, string, WatchEventDelegate{T}, CancellationToken)"/>
        /// throws when <see cref="KubernetesClient.WatchNamespacedObjectAsync{TObject, TList}(string, string, string, ListNamespacedObjectWithHttpMessagesAsync{TObject, TList}, WatchEventDelegate{TObject}, CancellationToken)"/>
        /// throws.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchAsync_Throws_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes
                .Setup(w => w.WatchNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>("fieldSelector", "labelSelector", "resourceVersion", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>>(), It.IsAny<WatchEventDelegate<V1Pod>>(), default))
                .ThrowsAsync(new HttpOperationException())
                .Verifiable();
            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            WatchEventDelegate<V1Pod> eventHandler = (eventType, value) => { return Task.FromResult(WatchResult.Stop); };

            // WatchAsync will throw when WatchNamespacedObjectAsync throws
            await Assert.ThrowsAsync<HttpOperationException>(() => client.WatchAsync("fieldSelector", "labelSelector", "resourceVersion", eventHandler, default)).ConfigureAwait(false);

            kubernetes.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.WatchAsync(string, string, string, WatchEventDelegate{T}, CancellationToken)"/>
        /// handles a bookmark event by updating the inner resource version and filtering out the bookmark event.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchAsync_HandlesBookmark_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // First invocation with 'resourceVersion' will return ServerDisconnected, second invocation with 'my-bookmark' will return ClientDisconnected
            kubernetes
                .Setup(w => w.WatchNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>("fieldSelector", "labelSelector", "resourceVersion", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>>(), It.IsAny<WatchEventDelegate<V1Pod>>(), default))
                .Callback<string, string, string, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>, WatchEventDelegate<V1Pod>, CancellationToken>(
                (fieldSelector, labelSelector, resourceVersion, listOperation, innerEventHandler, cancellationToken) =>
                {
                    Assert.Equal(WatchResult.Continue, innerEventHandler(k8s.WatchEventType.Bookmark, new V1Pod() { Metadata = new V1ObjectMeta() { ResourceVersion = "my-bookmark" } }).Result);
                })
                .ReturnsAsync(WatchExitReason.ServerDisconnected)
                .Verifiable();

            kubernetes
                .Setup(w => w.WatchNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>("fieldSelector", "labelSelector", "my-bookmark", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>>(), It.IsAny<WatchEventDelegate<V1Pod>>(), default))
                .ReturnsAsync(WatchExitReason.ClientDisconnected)
                .Verifiable();

            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            WatchEventDelegate<V1Pod> eventHandler = (eventType, value) => { throw new NotImplementedException(); };

            // WatchAsync will throw when WatchNamespacedObjectAsync throws
            await client.WatchAsync("fieldSelector", "labelSelector", "resourceVersion", eventHandler, default).ConfigureAwait(false);

            kubernetes.Verify();
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClient{T}.WatchAsync(string, string, string, WatchEventDelegate{T}, CancellationToken)"/>
        /// forwards non-bookmark events to the parent handler.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchAsync_ForwardsEvents_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            // First invocation with 'resourceVersion' will return ServerDisconnected, second invocation with 'my-bookmark' will return ClientDisconnected
            kubernetes
                .Setup(w => w.WatchNamespacedObjectAsync<V1Pod, ItemList<V1Pod>>("fieldSelector", "labelSelector", "resourceVersion", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>>(), It.IsAny<WatchEventDelegate<V1Pod>>(), default))
                .Callback<string, string, string, ListNamespacedObjectWithHttpMessagesAsync<V1Pod, ItemList<V1Pod>>, WatchEventDelegate<V1Pod>, CancellationToken>(
                (fieldSelector, labelSelector, resourceVersion, listOperation, innerEventHandler, cancellationToken) =>
                {
                    Assert.Equal(WatchResult.Stop, innerEventHandler(k8s.WatchEventType.Modified, new V1Pod() { Metadata = new V1ObjectMeta() { ResourceVersion = "my-bookmark" } }).Result);
                })
                .ReturnsAsync(WatchExitReason.ClientDisconnected)
                .Verifiable();

            var metadata = new KindMetadata(V1Pod.KubeGroup, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods");

            var client = new NamespacedKubernetesClient<V1Pod>(kubernetes.Object, metadata, NullLogger<NamespacedKubernetesClient<V1Pod>>.Instance);
            WatchEventDelegate<V1Pod> eventHandler = (eventType, value) => { return Task.FromResult(WatchResult.Stop); };

            // WatchAsync will throw when WatchNamespacedObjectAsync throws
            await client.WatchAsync("fieldSelector", "labelSelector", "resourceVersion", eventHandler, default).ConfigureAwait(false);

            kubernetes.Verify();
        }
    }
}
