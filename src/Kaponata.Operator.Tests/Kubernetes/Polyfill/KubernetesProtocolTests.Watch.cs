// <copyright file="KubernetesProtocolTests.Watch.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Moq;
using Nerdbank.Streams;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
#pragma warning disable CS0419 // Ambiguous reference in cref attribute

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method.
    /// </summary>
    public partial class KubernetesProtocolTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesProtocolTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public KubernetesProtocolTests(ITestOutputHelper output)
        {
            this.loggerFactory = LogFactory.Create(output);
            this.output = output;
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method validates the parameters passed to it.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchNamespacedObjectAsync_ValidatesArguments_Async()
        {
            var pod = new V1Pod() { Metadata = new V1ObjectMeta() { Name = "name", NamespaceProperty = "namespace" } };

            var protocol = new KubernetesProtocol(new DummyHandler(), this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            await Assert.ThrowsAsync<ArgumentNullException>("value", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(null, protocol.ListNamespacedPodWithHttpMessagesAsync, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(new V1Pod() { }, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(new V1Pod() { Metadata = new V1ObjectMeta() }, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "test" } }, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(new V1Pod() { Metadata = new V1ObjectMeta() { NamespaceProperty = "test" } }, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("listOperation", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(pod, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("eventHandler", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(pod, protocol.ListNamespacedPodWithHttpMessagesAsync, null, default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ArgumentNullException>("namespace", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(null, string.Empty, string.Empty, string.Empty, protocol.ListNamespacedPodWithHttpMessagesAsync, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("listOperation", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>("default", null, null, null, null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("eventHandler", () => protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>("default", null, null, null, protocol.ListNamespacedPodWithHttpMessagesAsync, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method immediately exists if it receives empty content.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchNamespacedObjectAsync_EmptyContent_Completes_Async()
        {
            var handler = new DummyHandler();
            handler.Responses.Enqueue(
                new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new WatchHttpContent(
                        new StringContent(string.Empty)),
                });

            Collection<(WatchEventType, V1Pod)> events = new Collection<(WatchEventType, V1Pod)>();

            var protocol = new KubernetesProtocol(handler, this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            var result = await protocol.WatchNamespacedObjectAsync<V1Pod, V1PodList>(
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta(name: "pod", namespaceProperty: "default", resourceVersion: "1"),
                },
                protocol.ListNamespacedPodWithHttpMessagesAsync,
                (eventType, result) =>
                {
                    events.Add((eventType, result));
                    return Task.FromResult(WatchResult.Continue);
                },
                default).ConfigureAwait(false);

            Assert.Equal(WatchExitReason.ServerDisconnected, result);
            Assert.Empty(events);
            Assert.Collection(
                handler.Requests,
                r =>
                {
                    Assert.Equal(new Uri("http://localhost/api/v1/namespaces/default/pods?fieldSelector=metadata.name%3Dpod&resourceVersion=1&watch=true"), r.RequestUri);
                });
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method can be cancelled.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchNamespacedObjectAsync_CancelsIfNeeded_Async()
        {
            var readTaskCompletion = new TaskCompletionSource<int>();

            var stream = new Mock<Stream>(MockBehavior.Strict);
            stream.Setup(s => s.CanSeek).Returns(false);
            stream.Setup(s => s.CanRead).Returns(true);
            stream
                .Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
                .Returns<Memory<byte>, CancellationToken>(async (memory, cancellationToken) => await readTaskCompletion.Task);
            stream
                .Setup(s => s.Close())
                .Callback(() => readTaskCompletion.TrySetException(new ObjectDisposedException("Stream", "Stream is being disposed, and all pending I/O cancelled")))
                .Verifiable();

            var handler = new DummyHandler();
            handler.Responses.Enqueue(
                new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new WatchHttpContent(
                        new StreamContent(stream.Object)),
                });

            Collection<(WatchEventType, V1Pod)> events = new Collection<(WatchEventType, V1Pod)>();

            var protocol = new KubernetesProtocol(handler, this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            var cts = new CancellationTokenSource();

            var watchTask = protocol.WatchNamespacedObjectAsync(
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta(name: "pod", namespaceProperty: "default", resourceVersion: "1"),
                },
                protocol.ListNamespacedPodWithHttpMessagesAsync,
                (eventType, result) =>
                {
                    events.Add((eventType, result));
                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            Assert.True(!watchTask.IsCompleted);
            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(() => watchTask).ConfigureAwait(false);

            stream.Verify();
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method throws an exception when a Kubernetes
        /// error occurs.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchNamespacedObjectAsync_ThrowsExceptionIfNeeded_Async()
        {
            var handler = new DummyHandler();
            handler.Responses.Enqueue(
                new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new WatchHttpContent(
                        new StringContent(
                            JsonConvert.SerializeObject(
                                new V1WatchEvent()
                                {
                                    Type = nameof(WatchEventType.Error),
                                    ObjectProperty =
                                        new V1Status()
                                        {
                                            Kind = "Status",
                                            Message = "ErrorMessage",
                                        },
                                }))),
                });

            Collection<(WatchEventType, V1Pod)> events = new Collection<(WatchEventType, V1Pod)>();

            var protocol = new KubernetesProtocol(handler, this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            var cts = new CancellationTokenSource();

            var ex = await Assert.ThrowsAsync<KubernetesException>(
                () => protocol.WatchNamespacedObjectAsync(
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta(name: "pod", namespaceProperty: "default", resourceVersion: "1"),
                },
                protocol.ListNamespacedPodWithHttpMessagesAsync,
                (eventType, result) =>
                {
                    events.Add((eventType, result));
                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token)).ConfigureAwait(false);
            Assert.Equal("ErrorMessage", ex.Message);
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method returns when the client
        /// returns <see cref="WatchResult.Stop"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchNamespacedObjectAsync_ClientCanStopLoop_Async()
        {
            using (var stream = new SimplexStream())
            using (var writer = new StreamWriter(stream))
            {
                var handler = new DummyHandler();
                handler.Responses.Enqueue(
                    new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new WatchHttpContent(
                            new StreamContent(stream)),
                    });

                Collection<(WatchEventType, V1Pod)> events = new Collection<(WatchEventType, V1Pod)>();

                var protocol = new KubernetesProtocol(handler, this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
                var cts = new CancellationTokenSource();

                var watchTask = protocol.WatchNamespacedObjectAsync(
                    new V1Pod()
                    {
                        Metadata = new V1ObjectMeta(name: "pod", namespaceProperty: "default", resourceVersion: "1"),
                    },
                    protocol.ListNamespacedPodWithHttpMessagesAsync,
                    (eventType, result) =>
                    {
                        events.Add((eventType, result));
                        return Task.FromResult(events.Count == 1 ? WatchResult.Continue : WatchResult.Stop);
                    },
                    cts.Token);

                Assert.True(!watchTask.IsCompleted);

                await writer.WriteAsync(
                    JsonConvert.SerializeObject(
                        new V1WatchEvent()
                        {
                            Type = nameof(WatchEventType.Deleted),
                            ObjectProperty = new V1Pod()
                            {
                                Metadata = new V1ObjectMeta()
                                {
                                    NamespaceProperty = "some-namespace",
                                    Name = "some-name",
                                },
                            },
                        }));
                await writer.WriteAsync('\n').ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);

                Assert.True(!watchTask.IsCompleted);

                await writer.WriteAsync(
                    JsonConvert.SerializeObject(
                        new V1WatchEvent()
                        {
                            Type = nameof(WatchEventType.Deleted),
                            ObjectProperty = new V1Pod()
                            {
                                Metadata = new V1ObjectMeta()
                                {
                                    NamespaceProperty = "some-namespace2",
                                    Name = "some-name2",
                                },
                            },
                        }));
                await writer.WriteAsync('\n').ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);

                var result = await watchTask;
                Assert.Equal(WatchExitReason.ClientDisconnected, result);

                Assert.Collection(
                    events,
                    e =>
                    {
                        Assert.Equal(WatchEventType.Deleted, e.Item1);
                        Assert.Equal("some-namespace", e.Item2.Metadata.NamespaceProperty);
                        Assert.Equal("some-name", e.Item2.Metadata.Name);
                    },
                    e =>
                    {
                        Assert.Equal(WatchEventType.Deleted, e.Item1);
                        Assert.Equal("some-namespace2", e.Item2.Metadata.NamespaceProperty);
                        Assert.Equal("some-name2", e.Item2.Metadata.Name);
                    });
            }
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method validates the parameters passed to it.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchCustomResourceDefinitionAsync_ValidatesArguments_Async()
        {
            var protocol = new KubernetesProtocol(new DummyHandler(), this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            await Assert.ThrowsAsync<ArgumentNullException>("value", () => protocol.WatchCustomResourceDefinitionAsync(null, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchCustomResourceDefinitionAsync(new V1CustomResourceDefinition(), (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ValidationException>(() => protocol.WatchCustomResourceDefinitionAsync(new V1CustomResourceDefinition() { Metadata = new V1ObjectMeta() }, (eventType, result) => Task.FromResult(WatchResult.Continue), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>("eventHandler", () => protocol.WatchCustomResourceDefinitionAsync(new V1CustomResourceDefinition() { Metadata = new V1ObjectMeta() { Name = "test" } }, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol.WatchNamespacedObjectAsync"/> method immediately exists if it receives empty content.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchCustomResourceDefinitionAsync_EmptyContent_Completes_Async()
        {
            var handler = new DummyHandler();
            handler.Responses.Enqueue(
                new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new WatchHttpContent(
                        new StringContent(string.Empty)),
                });

            Collection<(WatchEventType, V1CustomResourceDefinition)> events = new Collection<(WatchEventType, V1CustomResourceDefinition)>();

            var protocol = new KubernetesProtocol(handler, this.loggerFactory.CreateLogger<KubernetesProtocol>(), this.loggerFactory);
            var result = await protocol.WatchCustomResourceDefinitionAsync(
                new V1CustomResourceDefinition()
                {
                    Metadata = new V1ObjectMeta(name: "crd", namespaceProperty: "default", resourceVersion: "1"),
                },
                (eventType, result) =>
                {
                    events.Add((eventType, result));
                    return Task.FromResult(WatchResult.Continue);
                },
                default).ConfigureAwait(false);

            Assert.Equal(WatchExitReason.ServerDisconnected, result);
            Assert.Empty(events);
            Assert.Collection(
                handler.Requests,
                r =>
                {
                    Assert.Equal(new Uri("http://localhost/apis/apiextensions.k8s.io/v1/customresourcedefinitions?fieldSelector=metadata.name%3Dcrd&resourceVersion=1&watch=true"), r.RequestUri);
                });
        }

        // There are no further tests for WatchCustomResourceDefinitionAsync because the code is shared with WatchNamespacedObjectAsync via the WatchAsync method.
    }
}
