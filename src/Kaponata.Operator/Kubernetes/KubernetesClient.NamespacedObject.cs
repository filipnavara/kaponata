// <copyright file="KubernetesClient.NamespacedObject.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
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

        private async Task<HttpOperationResponse<TList>> ListNamespacedObjectAsync<TObject, TList>(
            KindMetadata kind,
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
            where TList : IItems<TObject>
            where TObject : IKubernetesObject<V1ObjectMeta>
        {
            Debug.Assert(allowWatchBookmarks == null, "Not supported by the generic Kubernetes API");
            Debug.Assert(resourceVersionMatch == null, "Not supported by the generic Kubernetes API");

            var operationResponse = await this.RunTaskAsync(this.protocol.ListNamespacedCustomObjectWithHttpMessagesAsync(
                kind.Group,
                kind.Version,
                namespaceParameter,
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

        private async Task<T> DeleteNamespacedObjectAsync<T>(
            KindMetadata kind,
            string name,
            string namespaceParameter,
            V1DeleteOptions body = null,
            string dryRun = null,
            int? gracePeriodSeconds = null,
            bool? orphanDependents = null,
            string propagationPolicy = null,
            string pretty = null,
            CancellationToken cancellationToken = default)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            using (var operationResponse = await this.protocol.DeleteNamespacedCustomObjectWithHttpMessagesAsync(
                kind.Group,
                kind.Version,
                namespaceParameter,
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
                var response = operationResponse.Response;

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    // It is actually very wrong to return a T from this method, but it's a problem that is deeply rooted
                    // within the Kubernetes client, see e.g.
                    // - https://github.com/kubernetes-client/csharp/issues/145
                    // - https://github.com/kubernetes-client/csharp/issues/475
                    var status = SafeJsonConvert.DeserializeObject<V1Status>(responseContent, this.protocol.DeserializationSettings);
                    return default;
                }
                catch (JsonException ex)
                {
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }
        }
    }
}
