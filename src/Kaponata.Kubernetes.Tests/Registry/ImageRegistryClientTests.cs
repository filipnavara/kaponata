// <copyright file="ImageRegistryClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="ImageRegistryClient"/> class.
    /// </summary>
    public class ImageRegistryClientTests
    {
        /// <summary>
        /// The <see cref="ImageRegistryClient" /> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ImageRegistryClient(null));
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.Dispose"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Dispose_Works_Async()
        {
            var httpClient = new HttpClient();
            var client = new ImageRegistryClient(httpClient);
            Assert.False(client.IsDisposed);

            client.Dispose();

            Assert.Throws<ObjectDisposedException>(() => httpClient.BaseAddress = new Uri("http://localhost:5000"));
            Assert.True(client.IsDisposed);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.DeleteBlobAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.DeleteManifestAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetBlobAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetManifestAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.ListTagsAsync(null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.PushBlobAsync(null, null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.PushManifestAsync(null, null, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushBlobAsync(string, Stream, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushBlobAsync_StandardFlow_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Post, "http://localhost:5000/v2/registry/blobs/uploads/")
                .WithContent(string.Empty)
                .Respond(
                    (request) =>
                    {
                        var response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.Accepted,
                        };

                        response.Headers.Location = new Uri("http://localhost:5000/v2/registry/blobs/uploads/65707092-6422-4b33-bba7-c655318616e9?_state=state_string");
                        return response;
                    });

            handler
                .Expect(HttpMethod.Put, "http://localhost:5000/v2/registry/blobs/uploads/65707092-6422-4b33-bba7-c655318616e9?_state=state_string&digest=sha256:0")
                .Respond(
                    (request) =>
                    {
                        var response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.Created,
                        };

                        response.Headers.Location = new Uri("http://localhost:5000/v2/devimg/blobs/sha256:0");
                        return response;
                    });

            using (var blobStream = new MemoryStream())
            {
                var httpClient = handler.ToHttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000");

                var client = new ImageRegistryClient(httpClient);

                var result = await client.PushBlobAsync("registry", blobStream, "sha256:0", default).ConfigureAwait(false);
                Assert.Equal(new Uri("http://localhost:5000/v2/devimg/blobs/sha256:0"), result);
            }

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushBlobAsync(string, Stream, string, CancellationToken)"/> throws when the registry is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushBlobAsync_InvalidRegistry_Throws_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Post, "http://localhost:5000/v2/REGISTRY/blobs/uploads/")
                .WithContent(string.Empty)
                .Respond(HttpStatusCode.NotFound);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);
            await Assert.ThrowsAsync<ImageRegistryException>(() => client.PushBlobAsync("REGISTRY", new MemoryStream(), "sha:0", default)).ConfigureAwait(false);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushBlobAsync(string, Stream, string, CancellationToken)"/> throws when the digest is invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushBlobAsync_InvalidDigest_Throws_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Post, "http://localhost:5000/v2/registry/blobs/uploads/")
                .WithContent(string.Empty)
                .Respond(
                    (request) =>
                    {
                        var response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.Accepted,
                        };

                        response.Headers.Location = new Uri("http://localhost:5000/v2/registry/blobs/uploads/65707092-6422-4b33-bba7-c655318616e9?_state=state_string");
                        return response;
                    });

            handler
                .Expect(HttpMethod.Put, "http://localhost:5000/v2/registry/blobs/uploads/65707092-6422-4b33-bba7-c655318616e9?_state=state_string&digest=sha256:0")
                .Respond(HttpStatusCode.BadRequest);

            using (var blobStream = new MemoryStream())
            {
                var httpClient = handler.ToHttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000");

                var client = new ImageRegistryClient(httpClient);
                await Assert.ThrowsAsync<ImageRegistryException>(() => client.PushBlobAsync("registry", blobStream, "sha256:0", default)).ConfigureAwait(false);
            }

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushBlobAsync(string, Stream, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushBlobAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushBlobAsync(null, Stream.Null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushBlobAsync(string.Empty, null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushBlobAsync(string.Empty, Stream.Null, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushManifestAsync(string, string, Stream, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushManifestAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Put, "http://localhost:5000/v2/registry/manifests/my-tag")
                .With((request) => request.Content is StreamContent)
                .With((request) => request.Content.Headers.ContentType.MediaType == "application/vnd.oci.image.manifest.v1+json")
                .Respond(
                    (request) =>
                    {
                        var response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.Created,
                        };

                        response.Headers.Location = new Uri("http://localhost:5000/v2/registry/manifests/my-tag");
                        return response;
                    });

            using (var blobStream = new MemoryStream())
            {
                var httpClient = handler.ToHttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000");

                var client = new ImageRegistryClient(httpClient);

                var result = await client.PushManifestAsync("registry", "my-tag", blobStream, default).ConfigureAwait(false);
                Assert.Equal(new Uri("http://localhost:5000/v2/registry/manifests/my-tag"), result);
            }

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushManifestAsync(string, string, Stream, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushManifestAsync_Error_Throws_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Put, "http://localhost:5000/v2/registry/manifests/my-tag")
                .With((request) => request.Content is StreamContent)
                .With((request) => request.Content.Headers.ContentType.MediaType == "application/vnd.oci.image.manifest.v1+json")
                .Respond(HttpStatusCode.InternalServerError, new StringContent("{\"errors\":[{\"code\":\"UNKNOWN\",\"message\":\"unknown error\",\"detail\":{}}]}\n", Encoding.UTF8, "application/json"));

            using (var blobStream = new MemoryStream())
            {
                var httpClient = handler.ToHttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000");

                var client = new ImageRegistryClient(httpClient);

                await Assert.ThrowsAsync<ImageRegistryException>(() => client.PushManifestAsync("registry", "my-tag", blobStream, default)).ConfigureAwait(false);
            }

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.PushManifestAsync(string, string, Stream, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PushManifestAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushManifestAsync(null, string.Empty, Stream.Null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushManifestAsync(string.Empty, null, Stream.Null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PushManifestAsync(string.Empty, string.Empty, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.ListTagsAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListTagsAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Get, "http://localhost:5000/v2/registry/tags/list")
                .Respond(HttpStatusCode.OK, new StringContent("{\"name\":\"registry\",\"tags\":[\"my-tag\"]}\n", Encoding.UTF8, "application/json"));

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            var tags = await client.ListTagsAsync("registry", default).ConfigureAwait(false);
            var tag = Assert.Single(tags);
            Assert.Equal("my-tag", tag);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.ListTagsAsync(string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListTagsAsync_NotFound_Throws_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Get, "http://localhost:5000/v2/invalid-registry/tags/list")
                .Respond(HttpStatusCode.NotFound, new StringContent("{\"errors\":[{\"code\":\"NAME_UNKNOWN\",\"message\":\"repository name not known to registry\",\"detail\":{\"name\":\"invalid-registry\"}}]}\n", Encoding.UTF8, "application/json"));

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            await Assert.ThrowsAsync<ImageRegistryException>(() => client.ListTagsAsync("invalid-registry", default)).ConfigureAwait(false);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.ListTagsAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListTagsAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.ListTagsAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.ListTagsAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetManifestAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Get, "http://localhost:5000/v2/registry/manifests/my-tag")
                .With((request) => request.Headers.Accept.Any(h => h.MediaType == "application/vnd.oci.image.manifest.v1+json"))
                .Respond(HttpStatusCode.OK, new StringContent("{\"schemaVersion\":2,\"config\":{\"mediaType\":\"\",\"digest\":\"sha256:a\",\"size\":1},\"layers\":[{\"mediaType\":\"\",\"digest\":\"sha256:b\",\"size\":2}]}", Encoding.UTF8, "application/json"));

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            var manifest = await client.GetManifestAsync("registry", "my-tag", default).ConfigureAwait(false);
            Assert.Equal(2, manifest.SchemaVersion);
            Assert.Equal(1, manifest.Config.Size);
            var layer = Assert.Single(manifest.Layers);
            Assert.Equal(2, layer.Size);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetManifestAsync(string, string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetManifestAsync_ThrowsOnError_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Get, "http://localhost:5000/v2/registry/manifests/my-tag")
                .With((request) => request.Headers.Accept.Any(h => h.MediaType == "application/vnd.oci.image.manifest.v1+json"))
                .Respond(HttpStatusCode.BadRequest);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            await Assert.ThrowsAsync<ImageRegistryException>(() => client.GetManifestAsync("registry", "my-tag", default)).ConfigureAwait(false);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetManifestAsync(string, string, CancellationToken)"/> returns <see langword="null"/>
        /// when the requested manifest could not be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetManifestAsync_RetursNullWhenNotFound_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Get, "http://localhost:5000/v2/registry/manifests/my-tag")
                .With((request) => request.Headers.Accept.Any(h => h.MediaType == "application/vnd.oci.image.manifest.v1+json"))
                .Respond(HttpStatusCode.NotFound);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            Assert.Null(await client.GetManifestAsync("registry", "my-tag", default).ConfigureAwait(false));

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetManifestAsync(string, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetManifestAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetManifestAsync(null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetManifestAsync(string.Empty, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetBlobAsync(string, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetBlobAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            using (MemoryStream content = new MemoryStream())
            {
                handler
                    .Expect(HttpMethod.Get, "http://localhost:5000/v2/registry/blobs/sha:0")
                    .Respond(HttpStatusCode.OK, new StreamContent(content));

                var httpClient = handler.ToHttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000");

                var client = new ImageRegistryClient(httpClient);

                var result = await client.GetBlobAsync("registry", "sha:0", default).ConfigureAwait(false);
                Assert.NotNull(result);
            }

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetBlobAsync(string, string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetBlobAsync_ThrowsOnError_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .When(HttpMethod.Get, "http://localhost:5000/v2/registry/blobs/sha:0")
                .Respond(HttpStatusCode.NotFound);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            await Assert.ThrowsAsync<ImageRegistryException>(() => client.GetBlobAsync("registry", "sha:0", default)).ConfigureAwait(false);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.GetBlobAsync(string, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetBlobAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetBlobAsync(null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetBlobAsync(string.Empty, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteBlobAsync(string, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteBlobAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Delete, "http://localhost:5000/v2/registry/blobs/sha:0")
                .Respond(HttpStatusCode.OK);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            // Should not throw
            await client.DeleteBlobAsync("registry", "sha:0", default).ConfigureAwait(false);

            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteBlobAsync(string, string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteBlobAsync_ThrowsOnError_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .When(HttpMethod.Delete, "http://localhost:5000/v2/registry/blobs/sha:0")
                .Respond(HttpStatusCode.NotFound);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            await Assert.ThrowsAsync<ImageRegistryException>(() => client.DeleteBlobAsync("registry", "sha:0", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteBlobAsync(string, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteBlobAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteBlobAsync(null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteBlobAsync(string.Empty, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteManifestAsync(string, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteManifestAsync_Works_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .Expect(HttpMethod.Delete, "http://localhost:5000/v2/registry/manifests/my-tag")
                .Respond(HttpStatusCode.OK);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            // Should not throw
            await client.DeleteManifestAsync("registry", "my-tag", default).ConfigureAwait(false);
            handler.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteManifestAsync(string, string, CancellationToken)"/> throws on errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteManifestAsync_ThrowsOnError_Async()
        {
            var handler = new MockHttpMessageHandler();

            handler
                .When(HttpMethod.Delete, "http://localhost:5000/v2/registry/manifests/my-tag")
                .Respond(HttpStatusCode.NotFound);

            var httpClient = handler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000");

            var client = new ImageRegistryClient(httpClient);

            await Assert.ThrowsAsync<ImageRegistryException>(() => client.DeleteManifestAsync("registry", "my-tag", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="ImageRegistryClient.DeleteManifestAsync(string, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteManifestAsync_ValidatesArguments_Async()
        {
            var client = new ImageRegistryClient(new HttpClient());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteManifestAsync(null, string.Empty, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.DeleteManifestAsync(string.Empty, null, default)).ConfigureAwait(false);
        }
    }
}
