// <copyright file="ImageRegistryExceptionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="ImageRegistryException"/> class.
    /// </summary>
    public class ImageRegistryExceptionTests
    {
        /// <summary>
        /// <see cref="ImageRegistryException.FromResponseAsync(HttpResponseMessage, CancellationToken)"/> works correctly
        /// when only a status code is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task FromStatusCode_NoContent_Works_Async()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var exception = await ImageRegistryException.FromResponseAsync(response, default).ConfigureAwait(false);

            Assert.Equal("An unexpected error occurred when sending a request to the remote server.", exception.Message);
            Assert.Empty(exception.Errors);
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        /// <summary>
        /// <see cref="ImageRegistryException.FromResponseAsync(HttpResponseMessage, CancellationToken)"/> works correctly
        /// when only a status code and text-based content is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task FromStatusCode_TextContent_Works_Async()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = new StringContent("NOT FOUND");

            var exception = await ImageRegistryException.FromResponseAsync(response, default).ConfigureAwait(false);

            Assert.Equal("NOT FOUND", exception.Message);
            Assert.Empty(exception.Errors);
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        /// <summary>
        /// <see cref="ImageRegistryException.FromResponseAsync(HttpResponseMessage, CancellationToken)"/> works correctly
        /// when a status code an JSON result is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task FromStatusCode_JsonContent_Works_Async()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent("{\"errors\":[{\"code\":\"DIGEST_INVALID\",\"message\":\"provided digest did not match uploaded content\",\"detail\":\"digest parsing failed\"}]}", Encoding.UTF8, "application/json");

            var exception = await ImageRegistryException.FromResponseAsync(response, default).ConfigureAwait(false);

            Assert.Equal("provided digest did not match uploaded content", exception.Message);
            Assert.Single(exception.Errors);
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }

        /// <summary>
        /// The <see cref="ImageRegistryException.ImageRegistryException(string)"/> constructor works correctly.
        /// </summary>
        [Fact]
        public void WithMessage_Works()
        {
            var ex = new ImageRegistryException("this is a test");
            Assert.Equal("this is a test", ex.Message);
        }
    }
}
