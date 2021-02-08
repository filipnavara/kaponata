// <copyright file="KubernetesMuxerSocketLocatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Operator.Kubernetes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="KubernetesMuxerSocketLocator"/> class.
    /// </summary>
    public class KubernetesMuxerSocketLocatorTests
    {
        /// <summary>
        /// The <see cref="KubernetesMuxerSocketLocator"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("kubernetes", () => new KubernetesMuxerSocketLocator(null, new V1Pod(), NullLogger<KubernetesMuxerSocketLocator>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("pod", () => new KubernetesMuxerSocketLocator(Mock.Of<IKubernetes>(), null, NullLogger<KubernetesMuxerSocketLocator>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesMuxerSocketLocator(Mock.Of<IKubernetes>(), new V1Pod(), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesMuxerSocketLocator(Mock.Of<IKubernetes>(), new V1Pod(), NullLogger<KubernetesMuxerSocketLocator>.Instance, null));
        }

        /// <summary>
        /// <see cref="KubernetesMuxerSocketLocator.ConnectToMuxerAsync"/> uses Kubernetes port forwarding.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ConnectToMuxerAsync_UsesPortForwarding_Async()
        {
            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "my-namespace",
                    Name = "usbmuxd-abcd",
                },
            };

            var websocket = Mock.Of<WebSocket>();
            var kubernetes = new Mock<IKubernetes>(MockBehavior.Strict);
            kubernetes
                .Setup(k => k.WebSocketNamespacedPodPortForwardAsync("usbmuxd-abcd", "my-namespace", new int[] { 27015 }, null, null, default))
                .ReturnsAsync(websocket);

            var locator = new KubernetesMuxerSocketLocator(kubernetes.Object, pod, NullLogger<KubernetesMuxerSocketLocator>.Instance, NullLoggerFactory.Instance);
            using (var stream = await locator.ConnectToMuxerAsync(default))
            {
                var portForwardStream = Assert.IsType<PortForwardStream>(stream);
                Assert.Same(websocket, portForwardStream.WebSocket);
            }

            kubernetes.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesMuxerSocketLocator.GetMuxerSocket"/> is not implemented.
        /// </summary>
        [Fact]
        public void GetMuxerSocket_Throws()
        {
            var locator = new KubernetesMuxerSocketLocator(Mock.Of<IKubernetes>(), new V1Pod(), NullLogger<KubernetesMuxerSocketLocator>.Instance, NullLoggerFactory.Instance);
            Assert.Throws<NotSupportedException>(() => locator.GetMuxerSocket());
        }
    }
}
