// <copyright file="KubernetesProtocolTests.PortForward.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="KubernetesProtocol.ConnectToPodPortAsync"/> method.
    /// </summary>
    public partial class KubernetesProtocolTests
    {
        /// <summary>
        /// <see cref="KubernetesProtocol.ConnectToPodPortAsync"/> validates its arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represens the asynchronous test.
        /// </returns>
        [Fact]
        public Task ConnectToPodPortAsync_ValidatesArguments_Async()
        {
            var handler = new DummyHandler();
            var client = new KubernetesProtocol(handler, NullLogger<KubernetesProtocol>.Instance, NullLoggerFactory.Instance);

            return Assert.ThrowsAsync<ArgumentNullException>(() => client.ConnectToPodPortAsync(null, 1, default));
        }

        /// <summary>
        /// <see cref="KubernetesProtocol.ConnectToPodPortAsync"/> creates a correctly configured stream.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ConnectToPodPortAsync_CreatesStream_Async()
        {
            var handler = new DummyHandler();
            var webSocketBuilder = new Mock<WebSocketBuilder>(MockBehavior.Strict);
            var webSocket = new Mock<WebSocket>();

            webSocketBuilder
                .Setup(b => b.BuildAndConnectAsync(new Uri("ws://localhost/api/v1/namespaces/default/pods/pod/portforward?ports=8080"), default))
                .ReturnsAsync(webSocket.Object)
                .Verifiable();

            var client = new KubernetesProtocol(handler, NullLogger<KubernetesProtocol>.Instance, NullLoggerFactory.Instance);
            client.CreateWebSocketBuilder = () => webSocketBuilder.Object;

            var stream = await client.ConnectToPodPortAsync(
                new V1Pod()
                {
                    Metadata = new V1ObjectMeta(name: "pod", namespaceProperty: "default", resourceVersion: "1"),
                },
                8080,
                default).ConfigureAwait(false);

            var portForwardStream = Assert.IsType<PortForwardStream>(stream);
            Assert.Same(webSocket.Object, portForwardStream.WebSocket);

            // No HTTP traffic
            Assert.Empty(handler.Requests);

            webSocketBuilder.Verify();
        }
    }
}
