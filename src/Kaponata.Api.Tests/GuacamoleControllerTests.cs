// <copyright file="GuacamoleControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Api.Guacamole;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="GuacamoleController"/> class.
    /// </summary>
    public class GuacamoleControllerTests
    {
        /// <summary>
        /// Retrieves tests data for the <see cref="AuthorizeAsync_SkipsDeviceWithoutVnc_Async"/> test.
        /// </summary>
        /// <returns>
        /// Tests data for the <see cref="AuthorizeAsync_SkipsDeviceWithoutVnc_Async"/> test.
        /// </returns>
        public static IEnumerable<object[]> AuthorizeAsync_SkipsDeviceWithoutVnc_Data()
        {
            yield return new object[]
            {
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "device",
                    },
                    Status = null,
                },
            };

            yield return new object[]
            {
                new MobileDevice()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "device",
                    },
                    Status = new MobileDeviceStatus()
                    {
                        VncHost = null,
                        VncPassword = null,
                    },
                },
            };
        }

        /// <summary>
        /// <see cref="GuacamoleController.AuthorizeAsync(AuthorizationRequest, CancellationToken)"/> always returns an empty list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AuthorizeAsync_ReturnsEmptyList_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, null, null, default))
                .ReturnsAsync(new ItemList<MobileDevice>() { Items = new List<MobileDevice>() });

            kubernetes.Setup(c => c.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var controller = new GuacamoleController(NullLogger<GuacamoleController>.Instance, kubernetes.Object);
            var result = await controller.AuthorizeAsync(new AuthorizationRequest(), default).ConfigureAwait(false);
            var objectResult = Assert.IsType<OkObjectResult>(result);

            var authorizationResult = Assert.IsType<AuthorizationResult>(objectResult.Value);
            Assert.True(authorizationResult.Authorized);
            Assert.Empty(authorizationResult.Configurations);
        }

        /// <summary>
        /// <see cref="GuacamoleController.AuthorizeAsync(AuthorizationRequest, CancellationToken)"/> always returns an empty list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AuthorizeAsync_ReturnsList_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, null, null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>()
                        {
                            new MobileDevice()
                            {
                                Metadata = new V1ObjectMeta()
                                {
                                    Name = "device",
                                },
                                Status = new MobileDeviceStatus()
                                {
                                    VncHost = "1.2.3.4",
                                    VncPassword = "abc",
                                },
                            },
                        },
                    });

            kubernetes.Setup(c => c.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var controller = new GuacamoleController(NullLogger<GuacamoleController>.Instance, kubernetes.Object);
            var result = await controller.AuthorizeAsync(new AuthorizationRequest(), default).ConfigureAwait(false);
            var objectResult = Assert.IsType<OkObjectResult>(result);

            var authorizationResult = Assert.IsType<AuthorizationResult>(objectResult.Value);
            Assert.True(authorizationResult.Authorized);
            var configuration = Assert.Single(authorizationResult.Configurations);
            Assert.Equal("device", configuration.Key);

            Assert.Equal("VNC", configuration.Value.Protocol);
            Assert.Collection(
                configuration.Value.Parameters,
                c =>
                {
                    Assert.Equal("hostname", c.Key);
                    Assert.Equal("1.2.3.4", c.Value);
                },
                c =>
                {
                    Assert.Equal("port", c.Key);
                    Assert.Equal(5900, c.Value);
                },
                c =>
                {
                    Assert.Equal("password", c.Key);
                    Assert.Equal(string.Empty, c.Value);
                });
        }

        /// <summary>
        /// <see cref="GuacamoleController.AuthorizeAsync(AuthorizationRequest, CancellationToken)"/> skips devices without valid VNC data.
        /// </summary>
        /// <param name="device">
        /// A device for which the VNC configuration is incomplete.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(AuthorizeAsync_SkipsDeviceWithoutVnc_Data))]
        public async Task AuthorizeAsync_SkipsDeviceWithoutVnc_Async(MobileDevice device)
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var deviceClient = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            deviceClient
                .Setup(d => d.ListAsync(null, null, null, null, default))
                .ReturnsAsync(
                    new ItemList<MobileDevice>()
                    {
                        Items = new List<MobileDevice>()
                        {
                            device,
                        },
                    });

            kubernetes.Setup(c => c.GetClient<MobileDevice>()).Returns(deviceClient.Object);

            var controller = new GuacamoleController(NullLogger<GuacamoleController>.Instance, kubernetes.Object);
            var result = await controller.AuthorizeAsync(new AuthorizationRequest(), default).ConfigureAwait(false);
            var objectResult = Assert.IsType<OkObjectResult>(result);

            var authorizationResult = Assert.IsType<AuthorizationResult>(objectResult.Value);
            Assert.True(authorizationResult.Authorized);
            Assert.Empty(authorizationResult.Configurations);
        }
    }
}
