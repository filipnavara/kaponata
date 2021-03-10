// <copyright file="KubernetesAdbSocketLocatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Operator.Kubernetes;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="KubernetesAdbSocketLocator"/> class.
    /// </summary>
    public class KubernetesAdbSocketLocatorTests
    {
        /// <summary>
        /// The <see cref="KubernetesAdbSocketLocator"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("kubernetes", () => new KubernetesAdbSocketLocator(null, new KubernetesAdbContext()));
            Assert.Throws<ArgumentNullException>("context", () => new KubernetesAdbSocketLocator(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="KubernetesAdbSocketLocator.ConnectToAdbAsync"/> uses Kubernetes port forwarding.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ConnectToAdbAsync_UsesPortForwarding_Async()
        {
            var pod = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "my-namespace",
                    Name = "adb-abcd",
                },
            };

            var stream = new Mock<Stream>();
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes
                .Setup(k => k.ConnectToPodPortAsync(pod, 5037, default))
                .ReturnsAsync(stream.Object);

            var locator = new KubernetesAdbSocketLocator(kubernetes.Object, new KubernetesAdbContext() { Pod = pod });
            using (var value = await locator.ConnectToAdbAsync(default))
            {
                Assert.Same(stream.Object, value);
            }

            kubernetes.Verify();
        }
    }
}
