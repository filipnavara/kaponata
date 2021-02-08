// <copyright file="NamespacedKubernetesClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Represents a generic Kubernetes client which provides strongly-typed access to namespaced objects.
    /// </summary>
    /// <typeparam name="T">
    /// The type of Kubernetes object being accessed using this client.
    /// </typeparam>
    public class NamespacedKubernetesClient<T>
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        private readonly KubernetesClient parent;
        private readonly KindMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespacedKubernetesClient{T}"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent <see cref="KubernetesClient"/> which provides access to the Kubernetes API.
        /// </param>
        /// <param name="metadata">
        /// Metadata on the current Kubernetes type, used to constructor the URLs used to access
        /// the object in the Kubernetes API.
        /// </param>
        public NamespacedKubernetesClient(KubernetesClient parent, KindMetadata metadata)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespacedKubernetesClient{T}"/> class.
        /// </summary>
        /// <remarks>
        /// Used for mocking purposes only.
        /// </remarks>
        protected NamespacedKubernetesClient()
        {
        }

        /// <summary>
        /// Asynchronously creates a new <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="value">
        /// The <typeparamref name="T"/> object object to create.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the newly created mobile device
        /// when completed.
        /// </returns>
        public virtual Task<T> CreateAsync(T value, CancellationToken cancellationToken)
        {
            return this.parent.CreateNamespacedValueAsync<T>(
                this.metadata,
                value,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously list or watch <typeparamref name="T"/> objects.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to list or watch objects.
        /// </param>
        /// <param name="continue">
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
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ItemList{T}"/> which represents the mobile devices which match the query.
        /// </returns>
        public async virtual Task<ItemList<T>> ListAsync(string @namespace, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            using (var operationResponse = await this.ListAsync(
                namespaceParameter: @namespace,
                continueParameter: @continue,
                fieldSelector: fieldSelector,
                labelSelector: labelSelector,
                cancellationToken: cancellationToken))
            {
                return operationResponse.Body;
            }
        }

        /// <summary>
        /// Asynchronously tries to read a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which the <typeparamref name="T"/> object is located.
        /// </param>
        /// <param name="name">
        /// The name which uniquely identifies the <typeparamref name="T"/> objectwithin the namespace.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the requested <typeparamref name="T"/> object, or
        /// <see langword="null"/> if the <typeparamref name="T"/> object does not exist.
        /// </returns>
        public virtual Task<T> TryReadAsync(string @namespace, string name, CancellationToken cancellationToken)
        {
            return this.TryReadAsync(@namespace, name, labelSelector: null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously tries to read a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which the <typeparamref name="T"/> object is located.
        /// </param>
        /// <param name="name">
        /// The name which uniquely identifies the <typeparamref name="T"/> objectwithin the namespace.
        /// </param>
        /// <param name="labelSelector">
        /// A label selector which the <typeparamref name="T"/> object must match.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the requested <typeparamref name="T"/> object, or
        /// <see langword="null"/> if the <typeparamref name="T"/> object does not exist.
        /// </returns>
        public virtual async Task<T> TryReadAsync(string @namespace, string name, string labelSelector, CancellationToken cancellationToken)
        {
            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var list = await this.parent.RunTaskAsync(this.ListAsync(namespaceParameter: @namespace, fieldSelector: $"metadata.name={name}", labelSelector: labelSelector, cancellationToken: cancellationToken)).ConfigureAwait(false);
            return list.Body.Items.SingleOrDefault();
        }

        /// <summary>
        /// Deletes an object if it exists.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to search for the object.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <param name="timeout">
        /// The amount of time alloted to the operation.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, and which returns the deleted object if an object was deleted.</returns>
        public virtual async Task<T> TryDeleteAsync(string @namespace, string name, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var value = await this.TryReadAsync(@namespace, name, cancellationToken).ConfigureAwait(false);

            if (value == null)
            {
                return default(T);
            }

            await this.DeleteAsync(
                value,
                new V1DeleteOptions(propagationPolicy: "Foreground"),
                timeout,
                cancellationToken).ConfigureAwait(false);

            return value;
        }

        /// <summary>
        /// Asynchronously deletes a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="value">
        /// The <typeparamref name="T"/> object to delete.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the <typeparamref name="T"/> object should be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task DeleteAsync(T value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.DeleteAsync(value, options: null, timeout, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes a <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="value">
        /// The <typeparamref name="T"/> object to delete.
        /// </param>
        /// <param name="options">
        /// Additional parameters which specify how the delete operation should be performed.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the <typeparamref name="T"/> object should be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task DeleteAsync(T value, V1DeleteOptions options, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.parent.DeleteNamespacedObjectAsync<T>(
                value,
                options,
                this.DeleteAsync,
                this.WatchAsync,
                timeout,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches <typeparamref name="T"/> objects.
        /// </summary>
        /// <param name="value">
        /// The object to watch.
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
        public Task<WatchExitReason> WatchAsync(
            T value,
            WatchEventDelegate<T> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.parent.WatchNamespacedObjectAsync<T, ItemList<T>>(
                 value,
                 this.ListAsync,
                 eventHandler,
                 cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches <typeparamref name="T"/> objects.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to watch for <typeparamref name="T"/> objects.
        /// </param>
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
        public virtual Task<WatchExitReason> WatchAsync(
            string @namespace,
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            WatchEventDelegate<T> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.parent.WatchNamespacedObjectAsync<T, ItemList<T>>(
                @namespace,
                fieldSelector,
                labelSelector,
                resourceVersion,
                this.ListAsync,
                eventHandler,
                cancellationToken);
        }

        /// <summary>
        /// Partially updates the an object.
        /// </summary>
        /// <param name="value">
        /// The value to patch.
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
        public virtual Task<T> PatchAsync(
            T value,
            JsonPatchDocument<T> patch,
            CancellationToken cancellationToken)
        {
            EnsureObjectAndMetadata(value);

            if (patch == null)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            return this.parent.PatchNamespacedObjectAsync<T>(
                this.metadata,
                value.Metadata.NamespaceProperty,
                value.Metadata.Name,
                new V1Patch(patch, V1Patch.PatchType.JsonPatch),
                cancellationToken);
        }

        /// <summary>
        /// Partially updates the status of an object.
        /// </summary>
        /// <param name="value">
        /// The value to patch.
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
        public virtual Task<T> PatchStatusAsync(
            T value,
            JsonPatchDocument<T> patch,
            CancellationToken cancellationToken)
        {
            EnsureObjectAndMetadata(value);

            if (patch == null)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            return this.parent.PatchNamespacedObjectStatusAsync<T>(
                this.metadata,
                value.Metadata.NamespaceProperty,
                value.Metadata.Name,
                new V1Patch(patch, V1Patch.PatchType.JsonPatch),
                cancellationToken);
        }

        private static void EnsureObjectAndMetadata(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Metadata == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata is required");
            }

            if (value.Metadata.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.Name is required");
            }

            if (value.Metadata.NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.NamespaceProperty is required");
            }
        }

        private Task<HttpOperationResponse<ItemList<T>>> ListAsync(
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
        {
            return this.parent.ListNamespacedObjectAsync<T, ItemList<T>>(
                this.metadata,
                namespaceParameter,
                allowWatchBookmarks,
                continueParameter,
                fieldSelector,
                labelSelector,
                limit,
                resourceVersion,
                resourceVersionMatch,
                timeoutSeconds,
                watch,
                pretty,
                customHeaders,
                cancellationToken);
        }

        private Task<T> DeleteAsync(
            string name,
            string namespaceParameter,
            V1DeleteOptions body = null,
            string dryRun = null,
            int? gracePeriodSeconds = null,
            bool? orphanDependents = null,
            string propagationPolicy = null,
            string pretty = null,
            CancellationToken cancellationToken = default)
        {
            return this.parent.DeleteNamespacedObjectAsync<T>(
                this.metadata,
                name,
                namespaceParameter,
                body,
                dryRun,
                gracePeriodSeconds,
                orphanDependents,
                propagationPolicy,
                pretty,
                cancellationToken);
        }
    }
}
