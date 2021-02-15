// <copyright file="WebDriverControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="WebDriverController"/> class.
    /// </summary>
    public class WebDriverControllerTests
    {
        /// <summary>
        /// The <see cref="WebDriverController"/> validates the arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new WebDriverController(null, NullLogger<WebDriverController>.Instance));
            Assert.Throws<ArgumentNullException>(() => new WebDriverController(Mock.Of<KubernetesWebDriver>(), null));
        }

        /// <summary>
        /// <see cref="WebDriverController.NewSessionAsync(NewSessionRequest, CancellationToken)"/> invokes
        /// the underlying driver.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task NewSessionAsync_InvokesDriver_Async()
        {
            var request = new NewSessionRequest();
            var response = new WebDriverResponse();

            var webDriver = new Mock<KubernetesWebDriver>(MockBehavior.Strict);
            webDriver
                .Setup(w => w.CreateSessionAsync(request, default))
                .ReturnsAsync(response);

            var controller = new WebDriverController(webDriver.Object, NullLogger<WebDriverController>.Instance);
            var result = await controller.NewSessionAsync(request, default).ConfigureAwait(false);

            Assert.Same(response, result.Value);
        }

        /// <summary>
        /// <see cref="WebDriverController.DeleteAsync(string, CancellationToken)"/> returns
        /// an error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task DeleteAsync_ReturnsError_Async()
        {
            const string sessionId = "session-id";
            var response = new WebDriverResponse();

            var webDriver = new Mock<KubernetesWebDriver>(MockBehavior.Strict);
            webDriver
                .Setup(w => w.DeleteSessionAsync(sessionId, default))
                .ReturnsAsync(response);

            var controller = new WebDriverController(webDriver.Object, NullLogger<WebDriverController>.Instance);
            var result = await controller.DeleteAsync(sessionId, default).ConfigureAwait(false);

            Assert.Same(response, result.Value);
        }

        /// <summary>
        /// <see cref="WebDriverController.StatusAsync(CancellationToken)"/> returns a
        /// status object.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task StatusAsync_ReturnsValue_Async()
        {
            var controller = new WebDriverController(Mock.Of<KubernetesWebDriver>(), NullLogger<WebDriverController>.Instance);
            var result = await controller.StatusAsync(default).ConfigureAwait(false);
            Assert.Equal(200, result.StatusCode);

            var response = Assert.IsType<WebDriverResponse>(result.Value);
            var status = Assert.IsType<WebDriverStatus>(response.Value);
        }
    }
}
