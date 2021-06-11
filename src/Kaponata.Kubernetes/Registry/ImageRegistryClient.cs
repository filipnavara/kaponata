// <copyright file="ImageRegistryClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// A client for container image registries.
    /// </summary>
    public class ImageRegistryClient : IDisposableObservable
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryClient"/> class.
        /// </summary>
        /// <param name="httpClient">
        /// The underlying <see cref="httpClient"/> which provides connectivity to the remote server.
        /// </param>
        public ImageRegistryClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.oci.image.manifest.v1+json"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryClient"/> class. Intended for mocking purposes only.
        /// </summary>
#nullable disable
        protected ImageRegistryClient()
        {
        }
#nullable restore

        /// <summary>
        /// Gets the <see cref="HttpClient"/> which is used to connect to the image registry.
        /// </summary>
        public HttpClient HttpClient => this.httpClient;

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Pushes a blob to the remote registry.
        /// </summary>
        /// <param name="repository">
        /// The name of the repository to which to push the blob.
        /// </param>
        /// <param name="blob">
        /// The blob to push to the remote registry.
        /// </param>
        /// <param name="digest">
        /// A hex-formatted digest string, prefixed by its algoritm.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and, when completed returns the URL at which the blob can be
        /// retrieved.</returns>
        public async virtual Task<Uri> PushBlobAsync(string repository, Stream blob, string digest, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            if (digest == null)
            {
                throw new ArgumentNullException(nameof(digest));
            }

            // https://github.com/opencontainers/distribution-spec/blob/main/spec.md#post-then-put
            // Obtain a session id (upload URL)
            var result = await this.httpClient.PostAsync($"/v2/{repository}/blobs/uploads/", new StringContent(string.Empty), cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.Accepted
                || result.Headers.Location == null)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            var location = new UriBuilder(result.Headers.Location);
            var query = HttpUtility.ParseQueryString(location.Query);
            query["digest"] = digest;
            location.Query = query.ToString();

            var content = new StreamContent(blob);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Headers.ContentLength = blob.Length;

            blob.Seek(0, SeekOrigin.Begin);
            result = await this.httpClient.PutAsync(location.Uri, content, cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.Created
                || result.Headers.Location == null)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            return result.Headers.Location;
        }

        /// <summary>
        /// Pushes a manifest to the remote registry.
        /// </summary>
        /// <param name="repository">
        /// The name of the repository to which to push the manifest.
        /// </param>
        /// <param name="reference">
        /// The reference (tag or digest) of the manifest.
        /// </param>
        /// <param name="manifest">
        /// A <see cref="Stream"/> which represents the manifest to push.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represens the asynchronous operation, and, when completed, returns the URL at which the manifest can be retreived.</returns>
        public async virtual Task<Uri> PushManifestAsync(string repository, string reference, Stream manifest, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            manifest.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(manifest);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.oci.image.manifest.v1+json");
            content.Headers.ContentLength = manifest.Length;

            var result = await this.httpClient.PutAsync($"/v2/{repository}/manifests/{reference}", content, cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.Created
                || result.Headers.Location == null)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            return result.Headers.Location;
        }

        /// <summary>
        /// Asynchronously lists all tags in a repository.
        /// </summary>
        /// <param name="repository">
        /// The name of the repository in which to list the tags.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and, when completed, returns a list of all tags.
        /// </returns>
        public async virtual Task<List<string>> ListTagsAsync(string repository, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var result = await this.httpClient.GetAsync($"/v2/{repository}/tags/list", cancellationToken).ConfigureAwait(false);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<string>();
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            var tagList = await result.Content.ReadFromJsonAsync<TagList>(null, cancellationToken).ConfigureAwait(false);
            return tagList!.Tags!;
        }

        /// <summary>
        /// Asynchronously retrieves a manifest.
        /// </summary>
        /// <param name="repository">
        /// The repository in which to look for the manifest.
        /// </param>
        /// <param name="reference">
        /// The digest or tag of the manifest.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and, when completed, returns
        /// the requested reference.
        /// </returns>
        public async virtual Task<Manifest?> GetManifestAsync(string repository, string reference, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            var result = await this.httpClient.GetAsync($"/v2/{repository}/manifests/{reference}", cancellationToken).ConfigureAwait(false);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            var manifest = await result.Content.ReadFromJsonAsync<Manifest>(null, cancellationToken).ConfigureAwait(false);
            return manifest;
        }

        /// <summary>
        /// Asynchronously retrieves a blob from the registry.
        /// </summary>
        /// <param name="repository">
        /// The repository in which to look for the blob.
        /// </param>
        /// <param name="digest">
        /// The digest of the blob.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and, when completed, returns the
        /// requested blob.
        /// </returns>
        public async virtual Task<Stream> GetBlobAsync(string repository, string digest, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (digest == null)
            {
                throw new ArgumentNullException(nameof(digest));
            }

            var result = await this.httpClient.GetAsync($"/v2/{repository}/blobs/{digest}", cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }

            var blob = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return blob;
        }

        /// <summary>
        /// Asynchronously deletes a manifest from a registry.
        /// </summary>
        /// <param name="repository">
        /// The repository in which to delete the manifest.
        /// </param>
        /// <param name="reference">
        /// The digest or tag of the manifest to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async virtual Task DeleteManifestAsync(string repository, string reference, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            var result = await this.httpClient.DeleteAsync($"/v2/{repository}/manifests/{reference}", cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously deletes a blob from the repository.
        /// </summary>
        /// <param name="repository">
        /// The name of the registry in which to delete the blob.
        /// </param>
        /// <param name="digest">
        /// The digest of the blob to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async virtual Task DeleteBlobAsync(string repository, string digest, CancellationToken cancellationToken)
        {
            Verify.NotDisposed(this);

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (digest == null)
            {
                throw new ArgumentNullException(nameof(digest));
            }

            var result = await this.httpClient.DeleteAsync($"/v2/{repository}/blobs/{digest}", cancellationToken).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw await ImageRegistryException.FromResponseAsync(result, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.httpClient?.Dispose();
            this.IsDisposed = true;
        }
    }
}
