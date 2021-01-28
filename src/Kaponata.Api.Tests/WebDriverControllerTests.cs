// <copyright file="WebDriverControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Microsoft.Extensions.Logging.Abstractions;
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
            Assert.Throws<ArgumentNullException>(() => new WebDriverController(null));
        }

        /// <summary>
        /// <see cref="WebDriverController.NewSessionAsync(object, CancellationToken)"/> returns
        /// an error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task NewSessionAsync_ReturnsError_Async()
        {
            var controller = new WebDriverController(NullLogger<WebDriverController>.Instance);
            var result = await controller.NewSessionAsync(null, default).ConfigureAwait(false);

            var webDriverResult = Assert.IsType<WebDriverErrorResult>(result);
            Assert.Equal(500, webDriverResult.StatusCode);

            var error = Assert.IsType<WebDriverErrorData>(webDriverResult.Value);
            Assert.Equal("session not created", error.Error);
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
            var controller = new WebDriverController(NullLogger<WebDriverController>.Instance);
            var result = await controller.DeleteAsync(null, default).ConfigureAwait(false);

            var webDriverResult = Assert.IsType<WebDriverErrorResult>(result);
            Assert.Equal(404, webDriverResult.StatusCode);

            var error = Assert.IsType<WebDriverErrorData>(webDriverResult.Value);
            Assert.Equal("invalid session id", error.Error);
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
            var controller = new WebDriverController(NullLogger<WebDriverController>.Instance);
            var result = await controller.StatusAsync(default).ConfigureAwait(false);

            var webDriverResult = Assert.IsType<WebDriverResult>(result);
            Assert.Equal(200, webDriverResult.StatusCode);

            var data = Assert.IsType<WebDriverData>(webDriverResult.Value);
            var status = Assert.IsType<WebDriverStatus>(data.Data);
        }
    }
}
