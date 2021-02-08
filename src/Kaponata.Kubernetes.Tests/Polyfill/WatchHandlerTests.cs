// <copyright file="WatchHandlerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes.Polyfill;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="WatchHandler"/> class.
    /// </summary>
    public class WatchHandlerTests
    {
        /// <summary>
        /// The <see cref="WatchHandler"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new WatchHandler(null));
        }

        /// <summary>
        /// The <see cref="WatchHandler"/> passes through requests for non-watch URLs.
        /// </summary>
        /// <param name="method">
        /// The HTTP method being used.
        /// </param>
        /// <param name="url">
        /// The URL being invoked.
        /// </param>
        /// <param name="statusCode">
        /// The HTTP status code of the response.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("GET", "http://localhost/test", HttpStatusCode.OK)]
        [InlineData("GET", "http://localhost/test?notonmywatch=true", HttpStatusCode.OK)]
        [InlineData("GET", "http://localhost/test?a=b&notonmywatch=true", HttpStatusCode.OK)]
        [InlineData("GET", "http://localhost/test?watch=true", HttpStatusCode.NotFound)]
        [InlineData("POST", "http://localhost/test?watch=true", HttpStatusCode.OK)]
        public async Task SendAsync_NonWatchUrl_PassThrough_Async(string method, string url, HttpStatusCode statusCode)
        {
            var innerHandler = new DummyHandler();
            var expectedResponse = new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent("test"),
            };

            innerHandler.Responses.Enqueue(expectedResponse);

            var client = new HttpClient(new WatchHandler(innerHandler));
            var response =
                method == "GET" ? await client.GetAsync(url).ConfigureAwait(false) : await client.PostAsync(url, new StringContent(string.Empty));

            Assert.Same(expectedResponse, response);
        }

        /// <summary>
        /// The <see cref="WatchHandler"/> intercepts requests for watch URLs.
        /// </summary>
        /// <param name="url">
        /// The URL being invoked.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData("http://localhost/test?watch=true")]
        [InlineData("http://localhost/test?a=1&watch=true")]
        public async Task SendAsync_WatchUrl_Intercepts_Async(string url)
        {
            var innerHandler = new DummyHandler();
            var expectedResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test"),
            };

            innerHandler.Responses.Enqueue(expectedResponse);

            var client = new HttpClient(new WatchHandler(innerHandler));
            var response = await client.GetAsync(url).ConfigureAwait(false);

            Assert.Same(expectedResponse, response);
            var watchContent = Assert.IsType<WatchHttpContent>(response.Content);
            Assert.IsType<StringContent>(watchContent.OriginalContent);
        }
    }
}
