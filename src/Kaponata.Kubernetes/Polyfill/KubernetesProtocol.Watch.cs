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
        public Task<WatchExitReason> WatchCustomResourceDefinitionAsync(
            V1CustomResourceDefinition value,
            WatchEventDelegate<V1CustomResourceDefinition> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.WatchObjectAsync(
                value,
                this.ListCustomResourceDefinitionWithHttpMessagesAsync,
                eventHandler,
                cancellationToken);
        }

        /// <inheritdoc/>
        public Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            TObject value,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Metadata == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata");
            }

            if (value.Metadata.NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.NamespaceProperty");
            }

            if (value.Metadata.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.Name");
            }

            return this.WatchNamespacedObjectAsync<TObject, TList>(
                @namespace: value.Metadata.NamespaceProperty,
                fieldSelector: $"metadata.name={value.Metadata.Name}",
                labelSelector: null,
                resourceVersion: value.Metadata.ResourceVersion,
                listOperation,
                eventHandler,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            string @namespace,
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            if (listOperation == null)
            {
                throw new ArgumentNullException(nameof(listOperation));
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            using (var response = await listOperation(
                namespaceParameter: @namespace,
                fieldSelector: fieldSelector,
                labelSelector: labelSelector,
                resourceVersion: resourceVersion,
                watch: true,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return await this.WatchAsync(response, eventHandler, cancellationToken).ConfigureAwait(false);
            }
        }

        private Task<WatchExitReason> WatchObjectAsync<TObject, TList>(
            TObject value,
            ListObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Metadata == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata");
            }

            if (value.Metadata.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.Name");
            }

            return this.WatchObjectAsync<TObject, TList>(
                fieldSelector: $"metadata.name={value.Metadata.Name}",
                value.Metadata.ResourceVersion,
                listOperation,
                eventHandler,
                cancellationToken);
        }

        private async Task<WatchExitReason> WatchObjectAsync<TObject, TList>(
            string fieldSelector,
            string resourceVersion,
            ListObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            using (var response = await listOperation(
                fieldSelector: fieldSelector,
                resourceVersion: resourceVersion,
                watch: true,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return await this.WatchAsync(response, eventHandler, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<WatchExitReason> WatchAsync<TObject, TList>(
            HttpOperationResponse<TList> response,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            using (var watchContent = (WatchHttpContent)response.Response.Content)
            using (var content = watchContent.OriginalContent)
            using (var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            {
                string line;

                // ReadLineAsync will return null when we've reached the end of the stream.
                try
                {
                    while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
                    {
                        var genericEvent =
                            SafeJsonConvert.DeserializeObject<Watcher<KubernetesObject>.WatchEvent>(line);

                        if (genericEvent.Object.Kind == "Status")
                        {
                            var statusEvent = SafeJsonConvert.DeserializeObject<Watcher<V1Status>.WatchEvent>(line);
                            this.logger.LogInformation("Stopped watching '{kind}' objects because of a status event with payload {status}", typeof(TObject).Name, statusEvent.Object);
                            throw new KubernetesException(statusEvent.Object);
                        }
                        else
                        {
                            var @event = SafeJsonConvert.DeserializeObject<Watcher<TObject>.WatchEvent>(line);
                            this.logger.LogDebug("Got an {event} event for a {kind} object", @event.Type, typeof(TObject).Name);

                            if (await eventHandler(@event.Type, @event.Object).ConfigureAwait(false) == WatchResult.Stop)
                            {
                                this.logger.LogInformation("Stopped watching '{kind}' objects because the client requested to stop watching.", typeof(TObject).Name);
                                return WatchExitReason.ClientDisconnected;
                            }
                        }
                    }
                }
                catch (Exception ex) when (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation("Stopped watching '{kind}' objects because a cancellation request was received.", typeof(TObject).Name);
                    throw new TaskCanceledException("The watch operation was cancelled.", ex);
                }

                this.logger.LogInformation("Stopped watching '{kind}' objects because the server closed the connection.", typeof(TObject).Name);
                return WatchExitReason.ServerDisconnected;
            }
        }
    }
}
