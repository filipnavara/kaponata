// <copyright file="KubernetesProtocol.Watch.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// Implements the watch methods on the <see cref="KubernetesProtocol"/> class.
    /// </summary>
    public partial class KubernetesProtocol
    {
        private delegate
            Task<HttpOperationResponse<TList>> ListNamespacedObjectWithHttpMessagesAsync<TObject, TList>(
            string namespaceParameter,
            bool? allowWatchBookmarks = null,
            string continueParameter = null,
            string fieldSelector = null,
            string labelSelector = null,
            int? limit = null,
            string resourceVersion = null,
            string resourceVersionMatch = null,
            int? timeoutSeconds = null,
            bool? watch = null,
            string pretty = null,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>;

        private delegate
            Task<HttpOperationResponse<TList>> ListObjectWithHttpMessagesAsync<TObject, TList>(
            bool? allowWatchBookmarks = null,
            string continueParameter = null,
            string fieldSelector = null,
            string labelSelector = null,
            int? limit = null,
            string resourceVersion = null,
            string resourceVersionMatch = null,
            int? timeoutSeconds = null,
            bool? watch = null,
            string pretty = null,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>;

        /// <inheritdoc/>
        public Task<WatchExitReason> WatchPodAsync(
            V1Pod value,
            Func<WatchEventType, V1Pod, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.WatchNamespacedObjectAsync(
                value,
                this.ListNamespacedPodWithHttpMessagesAsync,
                eventHandler,
                cancellationToken);
        }

        /// <inheritdoc/>
        public Task<WatchExitReason> WatchCustomResourceDefinitionAsync(
            V1CustomResourceDefinition value,
            Func<WatchEventType, V1CustomResourceDefinition, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.WatchObjectAsync(
                value,
                this.ListCustomResourceDefinitionWithHttpMessagesAsync,
                eventHandler,
                cancellationToken);
        }

        private async Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            TObject value,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            Func<WatchEventType, TObject, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            using (var response = await listOperation(
                value.Metadata.NamespaceProperty,
                fieldSelector: $"metadata.name={value.Metadata.Name}",
                resourceVersion: value.Metadata.ResourceVersion,
                watch: true,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return await this.WatchAsync(value, response, eventHandler, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<WatchExitReason> WatchObjectAsync<TObject, TList>(
            TObject value,
            ListObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            Func<WatchEventType, TObject, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            using (var response = await listOperation(
                fieldSelector: $"metadata.name={value.Metadata.Name}",
                resourceVersion: value.Metadata.ResourceVersion,
                watch: true,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return await this.WatchAsync(value, response, eventHandler, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<WatchExitReason> WatchAsync<TObject, TList>(
            TObject value,
            HttpOperationResponse<TList> response,
            Func<WatchEventType, TObject, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            using (var watchContent = (WatchHttpContent)response.Response.Content)
            using (var content = watchContent.OriginalContent)
            using (var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            {
                cancellationToken.Register(watchContent.Dispose);

                string line;

                // ReadLineAsync will return null when we've reached the end of the stream.
                try
                {
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        var genericEvent =
                            SafeJsonConvert.DeserializeObject<Watcher<KubernetesObject>.WatchEvent>(line);

                        if (genericEvent.Object.Kind == "Status")
                        {
                            var statusEvent = SafeJsonConvert.DeserializeObject<Watcher<V1Status>.WatchEvent>(line);
                            this.logger.LogInformation("Stopped watching '{kind}' object '{name}' because of a status event with payload {status}", value.Kind, value.Metadata.Name, statusEvent.Object);
                            throw new KubernetesException(statusEvent.Object);
                        }
                        else
                        {
                            var @event = SafeJsonConvert.DeserializeObject<Watcher<TObject>.WatchEvent>(line);
                            this.logger.LogDebug("Got an {event} event for object {object}", @event.Type, value.Metadata.Name);

                            if (await eventHandler(@event.Type, @event.Object).ConfigureAwait(false) == WatchResult.Stop)
                            {
                                this.logger.LogInformation("Stopped watching '{kind}' object '{name}' because the client requested to stop watching.", value.Kind, value.Metadata.Name);
                                return WatchExitReason.ClientDisconnected;
                            }
                        }
                    }
                }
                catch (Exception ex) when (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation("Stopped watching '{kind}' object '{name}' because a cancellation request was received.", value.Kind, value.Metadata.Name);
                    throw new TaskCanceledException("The watch operation was cancelled.", ex);
                }

                this.logger.LogInformation("Stopped watching '{kind}' object '{name}' because the server closed the connection.", value.Kind, value.Metadata.Name);
                return WatchExitReason.ServerDisconnected;
            }
        }
    }
}
