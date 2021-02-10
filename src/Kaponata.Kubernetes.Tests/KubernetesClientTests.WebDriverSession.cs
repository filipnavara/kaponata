// <copyright file="KubernetesClientTests.WebDriverSession.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
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

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the WebDriverSession-related code in <see cref="KubernetesClient"/>.
    /// </summary>
    /// <remarks>
    /// Because the WebDriverSession-related methods are small wrappers around custom object code,
    /// only positive scenarios are tested.
    /// </remarks>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// <see cref="KubernetesClient.CreateWebDriverSessionAsync(WebDriverSession, CancellationToken)"/> returns a <see cref="WebDriverSession"/> object
        /// when the operation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateWebDriverSessionAsync_Success_ReturnsObject_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                },
            };

            protocol.Setup(p => p.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                session,
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "webdriversessions" /* plural */,
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
                                    Content = new StringContent(JsonConvert.SerializeObject(Assert.IsType<WebDriverSession>(value))),
                                    StatusCode = HttpStatusCode.OK,
                                },
                            });
                    });

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var result = await client.CreateWebDriverSessionAsync(session, default).ConfigureAwait(false);
                Assert.NotNull(result);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.ListWebDriverSessionAsync(string, string, string, int?, CancellationToken)"/> returns a typed object
        /// when the operation completes successfully.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListWebDriverSessionAsync_Success_ReturnsTypedResponse_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());

            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "webdriversessions" /* plural */,
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
                                    new WebDriverSessionList()
                                    {
                                        Items = new WebDriverSession[]
                                        {
                                            new WebDriverSession()
                                            {
                                                Metadata = new V1ObjectMeta()
                                                {
                                                    Name = "session1",
                                                },
                                            },
                                            new WebDriverSession()
                                            {
                                                Metadata = new V1ObjectMeta()
                                                {
                                                    Name = "session2",
                                                },
                                            },
                                        },
                                    })),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var list = await client.ListWebDriverSessionAsync().ConfigureAwait(false);
                Assert.Collection(
                    list.Items,
                    d => { Assert.Equal("session1", d.Metadata.Name); },
                    d => { Assert.Equal("session2", d.Metadata.Name); });
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.TryReadWebDriverSessionAsync(string, CancellationToken)"/> returns a <see cref="WebDriverSession"/>
        /// object if the requested session exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TryReadWebDriverSessionAsync_SessionFound_ReturnsObject_Async()
        {
            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.ListNamespacedCustomObjectWithHttpMessagesAsync(
                "kaponata.io" /* group */,
                "v1alpha1" /* version */,
                "default" /* namespaceParameter */,
                "webdriversessions" /* plural */,
                null /* continueParameter */,
                "metadata.name=my-session" /* fieldSelector */,
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
                        Content = new StringContent("{ \"Items\": [ { \"Metadata\": { \"Name\": \"my-session\" } } ] }"),
                        StatusCode = HttpStatusCode.OK,
                    },
                });

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var device = await client.TryReadWebDriverSessionAsync("my-session", default).ConfigureAwait(false);
                Assert.Equal("my-session", device.Metadata.Name);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.WatchWebDriverSessionAsync(WebDriverSession, WatchEventDelegate{WebDriverSession}, CancellationToken)"/>
        /// delegates its work to <see cref="IKubernetesProtocol.WatchNamespacedObjectAsync{TObject, TList}(TObject, ListNamespacedObjectWithHttpMessagesAsync{TObject, TList}, WatchEventDelegate{TObject}, CancellationToken)"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WatchWebDriverSessionAsync_Works_Async()
        {
            WebDriverSession session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "default",
                    Name = "my-session",
                },
            };

            TaskCompletionSource<WatchExitReason> tcs = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(
                p => p.WatchNamespacedObjectAsync<WebDriverSession, ItemList<WebDriverSession>>(
                    session,
                    It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<WebDriverSession, ItemList<WebDriverSession>>>(),
                    It.IsAny<WatchEventDelegate<WebDriverSession>>(),
                    default))
                .Returns(tcs.Task);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.WatchWebDriverSessionAsync(
                    session,
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

        /// <summary>
        /// KubernetesClient.WatchWebDriverSessionAsync forwards requests to <see cref="IKubernetesProtocol"/>.
        /// </summary>
        [Fact]
        public void WatchWebDriverSessionAsync_ForwardRequests()
        {
            var tcs = new TaskCompletionSource<WatchExitReason>();
            var eventHandler = new WatchEventDelegate<WebDriverSession>((type, pod) => Task.FromResult(WatchResult.Continue));

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WatchNamespacedObjectAsync("default", "fieldSelector", "labelSelector", "resourceVersion", It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<WebDriverSession, ItemList<WebDriverSession>>>(), eventHandler, default))
                .Returns(tcs.Task);
            protocol.Setup(p => p.Dispose()).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                Assert.Same(tcs.Task, client.WatchWebDriverSessionAsync("fieldSelector", "labelSelector", "resourceVersion", eventHandler, default));
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesClient.DeleteWebDriverSessionAsync"/> returns when the WebDriver session is deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteWebDriverSessionAsync_DeviceDeleted_Returns_Async()
        {
            var session =
                new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                    },
                };

            WatchEventDelegate<WebDriverSession> callback = null;
            TaskCompletionSource<WatchExitReason> watchTask = new TaskCompletionSource<WatchExitReason>();

            var protocol = new Mock<IKubernetesProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DeserializationSettings).Returns(new JsonSerializerSettings());
            protocol.Setup(p => p.Dispose()).Verifiable();

            protocol
                .Setup(
                    p => p.WatchNamespacedObjectAsync<WebDriverSession, ItemList<WebDriverSession>>(
                        session,
                        It.IsAny<ListNamespacedObjectWithHttpMessagesAsync<WebDriverSession, ItemList<WebDriverSession>>>(),
                        It.IsAny<WatchEventDelegate<WebDriverSession>>(),
                        It.IsAny<CancellationToken>()))
                .Returns<WebDriverSession, ListNamespacedObjectWithHttpMessagesAsync<WebDriverSession, ItemList<WebDriverSession>>, WatchEventDelegate<WebDriverSession>, CancellationToken>(
                (session, list, watcher, ct) =>
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
                        "webdriversessions",
                        "my-session",
                        null, /* body */
                        null, /* gracePeriodSeconds */
                        null, /* orphanDependents */
                        null, /* propagationPolicy */
                        null, /* dryRun */
                        null, /* customHeaders */
                        default))
                .Returns(Task.FromResult(new HttpOperationResponse<object>() { Body = session, Response = new HttpResponseMessage(HttpStatusCode.OK) })).Verifiable();

            using (var client = new KubernetesClient(protocol.Object, KubernetesOptions.Default, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance))
            {
                var task = client.DeleteWebDriverSessionAsync(session, TimeSpan.FromMinutes(1), default);
                Assert.NotNull(callback);

                // The callback continues watching until the pod is deleted
                Assert.Equal(WatchResult.Continue, await callback(WatchEventType.Modified, session).ConfigureAwait(false));
                Assert.Equal(WatchResult.Stop, await callback(WatchEventType.Deleted, session).ConfigureAwait(false));
                watchTask.SetResult(WatchExitReason.ClientDisconnected);

                await task.ConfigureAwait(false);
            }

            protocol.Verify();
        }
    }
}
