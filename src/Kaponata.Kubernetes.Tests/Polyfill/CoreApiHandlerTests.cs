// <copyright file="CoreApiHandlerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Polyfill;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Polyfill
{
    /// <summary>
    /// Tests the <see cref="CoreApiHandler"/> class.
    /// </summary>
    public class CoreApiHandlerTests
    {
        /// <summary>
        /// The <see cref="CoreApiHandler"/> intercepts requests for the Core API and patches the request URL.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SendAsync_PatchedPodUrl_Async()
        {
            var innerHandler = new DummyHandler();
            var expectedResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test"),
            };

            innerHandler.Responses.Enqueue(expectedResponse);

            var client = new HttpClient(new CoreApiHandler(innerHandler));
            client.BaseAddress = new Uri("http://localhost");
            var response = await client.GetAsync("/apis/core/v1/").ConfigureAwait(false);

            Assert.Collection(
                innerHandler.Requests,
                r => Assert.Equal(new Uri("http://localhost/api/v1/"), r.RequestUri));
        }
    }
}
