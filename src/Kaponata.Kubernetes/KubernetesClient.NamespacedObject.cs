// <copyright file="KubernetesClient.NamespacedObject.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Contains helper methods for working with generic, namespaced objects.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <summary>
        /// Asynchronously creates a new, namespaced object.
        /// </summary>
        /// <param name="kind">
        /// A <see cref="KindMetadata"/> object which describes the type of object to generate,
        /// such as the API version or plural name.
        /// </param>
        /// <param name="value">
        /// The object to create.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the newly created object
        /// when completed.
        /// </returns>
        /// <typeparam name="T">
        /// The type of object to create.
        /// </typeparam>
        public virtual async Task<T> CreateNamespacedValueAsync<T>(
            KindMetadata kind,
            T value,
            CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>
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

            if (value.Metadata.NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.NamespaceProperty");
            }

            using (var operationResponse = await this.RunTaskAsync(this.protocol.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                value,
                kind.Group,
                kind.Version,
                value.Metadata.NamespaceProperty,
                kind.Plural,
                cancellationToken: cancellationToken)).ConfigureAwait(false))
            {
                return await this.GetResponseAsync<T>(operationResponse);
            }
        }

        /// <summary>
        /// Asynchronously lists namespaced objects.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of Kubernetes object to list.
        /// </typeparam>
        /// <typeparam name="TList">
        /// The type of a list which contains the result.
        /// </typeparam>
        /// <param name="kind">
        /// A <see cref="KindMetadata"/> object which describes the type of object to generate,
        /// such as the API version or plural name.
        /// </param>
        /// <param name="allowWatchBookmarks">
        /// allowWatchBookmarks requests watch events with type "BOOKMARK". Servers that
        /// do not implement bookmarks may ignore this flag and bookmarks are sent at the
        /// server's discretion. Clients should not assume bookmarks are returned at any
        /// specific interval, nor may they assume the server will send any BOOKMARK event
        /// during a session. If this is not a watch, this field is ignored. If the feature
        /// gate WatchBookmarks is not enabled in apiserver, this field is ignored.
        /// </param>
        /// <param name="continueParameter">
        /// The continue option should be set when retrieving more results from the server.
        /// Since this value is server defined, clients may only use the continue value from
        /// a previous query result with identical query parameters (except for the value
        /// of continue) and the server may reject a continue value it does not recognize.
        /// If the specified continue value is no longer valid whether due to expiration
        /// (generally five to fifteen minutes) or a configuration change on the server,
        /// the server will respond with a 410 ResourceExpired error together with a continue
        /// token. If the client needs a consistent list, it must restart their list without
        /// the continue field. Otherwise, the client may send another list request with
        /// the token received with the 410 error, the server will respond with a list starting
        /// from the next key, but from the latest snapshot, which is inconsistent from the
        /// previous list results - objects that are created, modified, or deleted after
        /// the first list request will be included in the response, as long as their keys
        /// are after the "next key". This field is not supported when watch is true. Clients
        /// may start a watch from the last resourceVersion value returned by the server
        /// and not miss any modifications.
        /// </param>
        /// <param name="fieldSelector">
        /// A selector to restrict the list of returned objects by their fields. Defaults
        /// to everything.
        /// </param>
        /// <param name="labelSelector">
        /// A selector to restrict the list of returned objects by their labels. Defaults
        /// to everything.
        /// </param>
        /// <param name="limit">
        /// limit is a maximum number of responses to return for a list call. If more items
        /// exist, the server will set the `continue` field on the list metadata to a value
        /// that can be used with the same initial query to retrieve the next set of results.
        /// Setting a limit may return fewer than the requested amount of items (up to zero
        /// items) in the event all requested objects are filtered out and clients should
        /// only use the presence of the continue field to determine whether more results
        /// are available. Servers may choose not to support the limit argument and will
        /// return all of the available results. If limit is specified and the continue field
        /// is empty, clients may assume that no more results are available. This field is
        /// not supported if watch is true. The server guarantees that the objects returned
        /// when using continue will be identical to issuing a single list call without a
        /// limit - that is, no objects created, modified, or deleted after the first request
        /// is issued will be included in any subsequent continued requests. This is sometimes
        /// referred to as a consistent snapshot, and ensures that a client that is using
        /// limit to receive smaller chunks of a very large result can ensure they see all
        /// possible objects. If objects are updated during a chunked list the version of
        /// the object that was present at the time the first list result was calculated
        /// is returned.
        /// </param>
        /// <param name="resourceVersion">
        /// When specified with a watch call, shows changes that occur after that particular
        /// version of a resource. Defaults to changes from the beginning of history. When
        /// specified for list: - if unset, then the result is returned from remote storage
        /// based on quorum-read flag; - if it's 0, then we simply return what we currently
        /// have in cache, no guarantee; - if set to non zero, then the result is at least
        /// as fresh as given rv.
        /// </param>
        /// <param name="resourceVersionMatch">
        /// Determines how resourceVersion is applied to list calls.
        /// It is highly recommended that resourceVersionMatch be set for list calls where
        /// resourceVersion is set. See <see href="https://kubernetes.io/docs/reference/using-api/api-concepts/#resource-versions"/>
        /// for details. Defaults to unset.
        /// </param>
        /// <param name="timeoutSeconds">
        /// Timeout for the list/watch call. This limits the duration of the call, regardless
        /// of any activity or inactivity.
        /// </param>
        /// <param name="watch">
        /// Watch for changes to the described resources and return them as a stream of add,
        /// update, and remove notifications.
        /// </param>
        /// <param name="pretty">
        /// If 'true', then the output is pretty printed.
        /// </param>
        /// <param name="customHeaders">
        /// The headers that will be added to request.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async virtual Task<HttpOperationResponse<TList>> ListNamespacedObjectAsync<TObject, TList>(
            KindMetadata kind,
            bool? allowWatchBookmarks = null,
            string? continueParameter = null,
            string? fieldSelector = null,
            string? labelSelector = null,
            int? limit = null,
            string? resourceVersion = null,
            string? resourceVersionMatch = null,
            int? timeoutSeconds = null,
            bool? watch = null,
            string? pretty = null,
            Dictionary<string, List<string>>? customHeaders = null,
            CancellationToken cancellationToken = default)
            where TList : IItems<TObject>
            where TObject : IKubernetesObject<V1ObjectMeta>
        {
            Debug.Assert(allowWatchBookmarks == null, "Not supported by the generic Kubernetes API");
            Debug.Assert(resourceVersionMatch == null, "Not supported by the generic Kubernetes API");

            var operationResponse = await this.RunTaskAsync(this.protocol.ListNamespacedCustomObjectWithHttpMessagesAsync(
                kind.Group,
                kind.Version,
                this.options.Value.Namespace,
                kind.Plural,
                continueParameter: continueParameter,
                fieldSelector: fieldSelector,
                labelSelector: labelSelector,
                limit: limit,
                resourceVersion: resourceVersion,
                timeoutSeconds: timeoutSeconds,
                watch: watch,
                pretty: pretty,
                customHeaders: customHeaders,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            var typedOperationResponse = new HttpOperationResponse<TList>()
            {
                Request = operationResponse.Request,
                Response = operationResponse.Response,
            };

            if (operationResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await operationResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    typedOperationResponse.Body = SafeJsonConvert.DeserializeObject<TList>(responseContent, this.protocol.DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }

            return typedOperationResponse;
        }

        /// <summary>
        /// Deletes the specified namespace scoped custom object.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to delete.
        /// </typeparam>
        /// <param name="kind">
        /// The group, version and kind information which represents the object type.
        /// </param>
        /// <param name="name">
        /// The name of the object to delete.
        /// </param>
        /// <param name="body">
        /// Specific delete options.
        /// </param>
        /// <param name="dryRun">
        /// When present, indicates that modifications should not be persisted. An invalid
        /// or unrecognized dryRun directive will result in an error response and no further
        /// processing of the request. Valid values are: - All: all dry run stages will be
        /// processed.
        /// </param>
        /// <param name="gracePeriodSeconds">
        /// The duration in seconds before the object should be deleted. Value must be non-negative
        /// integer. The value zero indicates delete immediately. If this value is nil, the
        /// default grace period for the specified type will be used. Defaults to a per object
        /// value if not specified. zero means delete immediately.
        /// </param>
        /// <param name="orphanDependents">
        /// Deprecated: please use the PropagationPolicy, this field will be deprecated in
        /// 1.7. Should the dependent objects be orphaned. If true/false, the "orphan" finalizer
        /// will be added to/removed from the object's finalizers list. Either this field
        /// or PropagationPolicy may be set, but not both.
        /// </param>
        /// <param name="propagationPolicy">
        /// Whether and how garbage collection will be performed. Either this field or OrphanDependents
        /// may be set, but not both. The default policy is decided by the existing finalizer
        /// set in the metadata.finalizers and the resource-specific default policy.
        /// </param>
        /// <param name="pretty">
        /// A value indicating whether the output should be pretty-printed.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous request.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task<T?> DeleteNamespacedObjectAsync<T>(
            KindMetadata kind,
            string name,
            V1DeleteOptions? body = null,
            string? dryRun = null,
            int? gracePeriodSeconds = null,
            bool? orphanDependents = null,
            string? propagationPolicy = null,
            string? pretty = null,
            CancellationToken cancellationToken = default)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            using (var operationResponse = await this.protocol.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                kind.Group,
                kind.Version,
                this.options.Value.Namespace,
                kind.Plural,
                name,
                body,
                gracePeriodSeconds,
                orphanDependents,
                propagationPolicy,
                dryRun,
                customHeaders: null,
                cancellationToken).ConfigureAwait(false))
            {
                // It is actually very wrong to return a T from this method, but it's a problem that is deeply rooted
                // within the Kubernetes client, see e.g.
                // - https://github.com/kubernetes-client/csharp/issues/145
                // - https://github.com/kubernetes-client/csharp/issues/475
                var status = await this.GetResponseAsync<V1Status>(operationResponse);
                return default;
            }
        }

        /// <summary>
        /// Asynchronously watches a namespaced object.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to watch.
        /// </typeparam>
        /// <typeparam name="TList">
        /// The type of a list of <typeparamref name="TObject"/> objects.
        /// </typeparam>
        /// <param name="value">
        /// The object to watch.
        /// </param>
        /// <param name="listOperation">
        /// A delegate which lists all objects.
        /// </param>
        /// <param name="eventHandler">
        /// An handler which processes a watch event, and lets the watcher know whether
        /// to continue watching or not.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the watch operation. The task completes
        /// when the watcher stops watching for events. The <see cref="WatchExitReason"/>
        /// return value describes why the watcher stopped. The task errors if the watch
        /// loop errors.
        /// </returns>
        public Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            TObject value,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            return this.protocol.WatchNamespacedObjectAsync<TObject, TList>(
                value,
                listOperation,
                eventHandler,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches namespaced objects.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to watch.
        /// </typeparam>
        /// <typeparam name="TList">
        /// The type of a list of <typeparamref name="TObject"/> objects.
        /// </typeparam>
        /// <param name="fieldSelector">
        /// A selector to restrict the list of returned objects by their fields. Defaults
        /// to everything.
        /// </param>
        /// <param name="labelSelector">
        /// A selector to restrict the list of returned objects by their labels. Defaults
        /// to everything.
        /// </param>
        /// <param name="resourceVersion">
        /// resourceVersion sets a constraint on what resource versions a request may be
        /// served from. See <see href="https://kubernetes.io/docs/reference/using-api/api-concepts/#resource-versions"/>
        /// for details. Defaults to unset.
        /// </param>
        /// <param name="listOperation">
        /// A delegate which lists all objects.
        /// </param>
        /// <param name="eventHandler">
        /// An handler which processes a watch event, and lets the watcher know whether
        /// to continue watching or not.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the watch operation. The task completes
        /// when the watcher stops watching for events. The <see cref="WatchExitReason"/>
        /// return value describes why the watcher stopped. The task errors if the watch
        /// loop errors.
        /// </returns>
        public Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>
        {
            return this.protocol.WatchNamespacedObjectAsync<TObject, TList>(
                this.options.Value.Namespace,
                fieldSelector,
                labelSelector,
                resourceVersion,
                listOperation,
                eventHandler,
                cancellationToken);
        }

        /// <summary>
        /// Partially updates the specified namespace scoped custom object.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to patch.
        /// </typeparam>
        /// <param name="metadata">
        /// Metadata which describes the object type.
        /// </param>
        /// <param name="namespace">
        /// The namespace of the object to patch.
        /// </param>
        /// <param name="name">
        /// The name of the object to patch.
        /// </param>
        /// <param name="patch">
        /// The patch to apply to the object.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task<T> PatchNamespacedObjectAsync<T>(
            KindMetadata metadata,
            string @namespace,
            string name,
            V1Patch patch,
            CancellationToken cancellationToken)
        {
            using (var operationResponse = await this.RunTaskAsync(this.protocol.PatchNamespacedCustomObjectWithHttpMessagesAsync(
                patch,
                metadata.Group,
                metadata.Version,
                @namespace,
                metadata.Plural,
                name,
                null,
                null,
                null,
                null,
                cancellationToken)).ConfigureAwait(false))
            {
                return await this.GetResponseAsync<T>(operationResponse);
            }
        }

        /// <summary>
        /// Partially update status of the specified namespace scoped custom object.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to patch.
        /// </typeparam>
        /// <param name="metadata">
        /// Metadata which describes the object type.
        /// </param>
        /// <param name="name">
        /// The name of the object to patch.
        /// </param>
        /// <param name="patch">
        /// The patch to apply to the object.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task<T> PatchNamespacedObjectStatusAsync<T>(
            KindMetadata metadata,
            string name,
            V1Patch patch,
            CancellationToken cancellationToken)
        {
            using (var operationResponse = await this.RunTaskAsync(this.protocol.PatchNamespacedCustomObjectStatusWithHttpMessagesAsync(
                patch,
                metadata.Group,
                metadata.Version,
                this.options.Value.Namespace,
                metadata.Plural,
                name,
                null,
                null,
                null,
                null,
                cancellationToken)).ConfigureAwait(false))
            {
                return await this.GetResponseAsync<T>(operationResponse);
            }
        }

        private async Task<T> GetResponseAsync<T>(HttpOperationResponse<object> operationResponse)
        {
            var response = operationResponse.Response;

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                return SafeJsonConvert.DeserializeObject<T>(responseContent, this.protocol.DeserializationSettings);
            }
            catch (JsonException ex)
            {
                throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
            }
        }
    }
}
