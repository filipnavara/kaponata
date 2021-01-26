// <copyright file="KubernetesClient.MobileDevice.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Implements the <see cref="MobileDevice"/> operations.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <summary>
        /// Asynchronously creates a new mobile device.
        /// </summary>
        /// <param name="value">
        /// The mobile device to create.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the newly created mobile device
        /// when completed.
        /// </returns>
        public virtual async Task<MobileDevice> CreateMobileDeviceAsync(MobileDevice value, CancellationToken cancellationToken)
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
                MobileDevice.KubeGroup,
                MobileDevice.KubeVersion,
                value.Metadata.NamespaceProperty,
                MobileDevice.KubePlural,
                cancellationToken: cancellationToken)).ConfigureAwait(false))
            {
                var response = operationResponse.Response;

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    return SafeJsonConvert.DeserializeObject<MobileDevice>(responseContent, this.protocol.DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }
        }

        /// <summary>
        /// Asynchronously list or watch <see cref="MobileDevice"/> objects.
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
        /// A <see cref="MobileDeviceList"/> which represents the mobile devices which match the query.
        /// </returns>
        public async virtual Task<MobileDeviceList> ListMobileDeviceAsync(string @namespace, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            using (var operationResponse = await this.ListMobileDeviceAsync(
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
        /// Asynchronously tries to read a mobile device.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which the mobile device is located.
        /// </param>
        /// <param name="name">
        /// The name which uniquely identifies the mobile device within the namespace.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the requested mobile device, or
        /// <see langword="null"/> if the mobile device does not exist.
        /// </returns>
        public virtual async Task<MobileDevice> TryReadMobileDeviceAsync(string @namespace, string name, CancellationToken cancellationToken)
        {
            var list = await this.RunTaskAsync(this.ListMobileDeviceAsync(namespaceParameter: @namespace, fieldSelector: $"metadata.name={name}", cancellationToken: cancellationToken)).ConfigureAwait(false);
            return list.Body.Items.SingleOrDefault();
        }

        /// <summary>
        /// Asynchronously deletes a pod.
        /// </summary>
        /// <param name="value">
        /// The pod to delete.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the pod should be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task DeleteMobileDeviceAsync(MobileDevice value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.DeleteNamespacedObjectAsync<MobileDevice>(
                value,
                this.DeleteNamespacedMobileDeviceAsync,
                this.WatchMobileDeviceAsync,
                timeout,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches <see cref="MobileDevice"/> objects.
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
        public Task<WatchExitReason> WatchMobileDeviceAsync(
            MobileDevice value,
            Func<WatchEventType, MobileDevice, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.protocol.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                 value,
                 this.ListMobileDeviceAsync,
                 eventHandler,
                 cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches <see cref="MobileDevice"/> objects.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to watch for <see cref="MobileDevice"/> objects.
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
        public Task<WatchExitReason> WatchMobileDeviceAsync(
            string @namespace,
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            Func<WatchEventType, MobileDevice, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.protocol.WatchNamespacedObjectAsync<MobileDevice, MobileDeviceList>(
                @namespace,
                fieldSelector,
                labelSelector,
                resourceVersion,
                this.ListMobileDeviceAsync,
                eventHandler,
                cancellationToken);
        }

        private async Task<HttpOperationResponse<MobileDeviceList>> ListMobileDeviceAsync(
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
            Debug.Assert(allowWatchBookmarks == null, "Not supported by the generic Kubernetes API");
            Debug.Assert(resourceVersionMatch == null, "Not supported by the generic Kubernetes API");

            var operationResponse = await this.RunTaskAsync(this.protocol.ListNamespacedCustomObjectWithHttpMessagesAsync(
                MobileDevice.KubeGroup,
                MobileDevice.KubeVersion,
                namespaceParameter,
                MobileDevice.KubePlural,
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

            var typedOperationResponse = new HttpOperationResponse<MobileDeviceList>()
            {
                Request = operationResponse.Request,
                Response = operationResponse.Response,
            };

            if (operationResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await operationResponse.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    typedOperationResponse.Body = SafeJsonConvert.DeserializeObject<MobileDeviceList>(responseContent, this.protocol.DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }

            return typedOperationResponse;
        }

        private async Task<MobileDevice> DeleteNamespacedMobileDeviceAsync(
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
            using (var operationResponse = await this.protocol.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                MobileDevice.KubeGroup,
                MobileDevice.KubeVersion,
                namespaceParameter,
                MobileDevice.KubePlural,
                name,
                body,
                gracePeriodSeconds,
                orphanDependents,
                propagationPolicy,
                dryRun,
                customHeaders: null,
                cancellationToken).ConfigureAwait(false))
            {
                var response = operationResponse.Response;

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    // It is actually very wrong to return a MobileDevice from this method, but it's a problem that is deeply rooted
                    // within the Kubernetes client, see e.g.
                    // - https://github.com/kubernetes-client/csharp/issues/145
                    // - https://github.com/kubernetes-client/csharp/issues/475
                    var status = SafeJsonConvert.DeserializeObject<V1Status>(responseContent, this.protocol.DeserializationSettings);
                    return null;
                }
                catch (JsonException ex)
                {
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }
        }
    }
}
