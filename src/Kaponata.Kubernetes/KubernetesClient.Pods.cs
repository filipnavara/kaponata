// <copyright file="KubernetesClient.Pods.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Contains methods for working with Kubernetes pods.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <summary>
        /// Asynchronously creates a new pod.
        /// </summary>
        /// <param name="value">
        /// The pod to create.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the newly created pod
        /// when completed.
        /// </returns>
        public virtual Task<V1Pod> CreatePodAsync(V1Pod value, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Metadata?.NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.NamespaceProperty");
            }

            return this.RunTaskAsync(this.protocol.CreateNamespacedPodAsync(value, value.Metadata.NamespaceProperty, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Asynchronously tries to read a pod.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which the pod is located.
        /// </param>
        /// <param name="name">
        /// The name which uniquely identifies the pod within the namespace.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the requested pod, or
        /// <see langword="null"/> if the pod does not exist.
        /// </returns>
        public virtual async Task<V1Pod?> TryReadPodAsync(string @namespace, string name, CancellationToken cancellationToken)
        {
            var list = await this.RunTaskAsync(this.protocol.ListNamespacedPodAsync(@namespace, fieldSelector: $"metadata.name={name}", cancellationToken: cancellationToken)).ConfigureAwait(false);
            return list.Items.SingleOrDefault();
        }

        /// <summary>
        /// Asynchronously lists <see cref="V1Pod"/> objects.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to list <see cref="V1Pod"/> objects.
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
        /// A <see cref="V1PodList"/> which represents the mobile devices which match the query.
        /// </returns>
        public async virtual Task<V1PodList> ListPodAsync(string @namespace, string? @continue = null, string? fieldSelector = null, string? labelSelector = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            using (var operationResponse = await this.RunTaskAsync(this.protocol.ListNamespacedPodWithHttpMessagesAsync(
                namespaceParameter: @namespace,
                continueParameter: @continue,
                fieldSelector: fieldSelector,
                labelSelector: labelSelector,
                limit: limit,
                cancellationToken: cancellationToken)))
            {
                return operationResponse.Body;
            }
        }

        /// <summary>
        /// Asynchronously watches a pod.
        /// </summary>
        /// <param name="value">
        /// The pod being watched.
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
        public virtual Task<WatchExitReason> WatchPodAsync(
            V1Pod value,
            WatchEventDelegate<V1Pod> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.protocol.WatchNamespacedObjectAsync(
                value,
                this.protocol.ListNamespacedPodWithHttpMessagesAsync,
                eventHandler,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously watches pods.
        /// </summary>
        /// <param name="namespace">
        /// The namespace in which to watch for <see cref="V1Pod"/> objects.
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
        public virtual Task<WatchExitReason> WatchPodAsync(
            string @namespace,
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            WatchEventDelegate<V1Pod> eventHandler,
            CancellationToken cancellationToken)
        {
            return this.protocol.WatchNamespacedObjectAsync(
                @namespace,
                fieldSelector: fieldSelector,
                labelSelector: labelSelector,
                resourceVersion: resourceVersion,
                this.protocol.ListNamespacedPodWithHttpMessagesAsync,
                eventHandler,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for a pod to enter the running phase.
        /// </summary>
        /// <param name="value">
        /// The pod which should enter the running state.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the pod should enter the running state.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="V1Pod"/>.
        /// </returns>
        public virtual async Task<V1Pod> WaitForPodRunningAsync(V1Pod value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (IsRunning(value))
            {
                return value;
            }

            if (HasFailed(value, out Exception? ex))
            {
                throw ex;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = this.WatchPodAsync(
                value,
                eventHandler: (type, updatedPod) =>
                {
                    value = updatedPod;

                    if (type == WatchEventType.Deleted)
                    {
                        throw new KubernetesException($"The pod {value.Metadata.Name} was deleted.");
                    }

                    // Wait for the pod to be ready. Since we work with init containers, merely waiting for the pod to have an IP address is not enough -
                    // we need both 'Running' status _and_ and IP address
                    if (IsRunning(value))
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    if (HasFailed(value, out Exception? ex))
                    {
                        throw ex;
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The pod {value.Metadata.Name} did not transition to the completed state within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching pod {value.Metadata.Name}.");
            }

            return value;
        }

        /// <summary>
        /// Creates a <see cref="HttpClient"/> which connects to a port on a specific pod.
        /// </summary>
        /// <param name="pod">
        /// The pod on which the HTTP server is running.
        /// </param>
        /// <param name="port">
        /// The port on the pod to which to connect.
        /// </param>
        /// <returns>
        /// A <see cref="HttpClient"/> which can be used to connect to the port on the pod.
        /// </returns>
        public virtual HttpClient CreatePodHttpClient(V1Pod pod, int port)
        {
            EnsureObjectMetadata(pod);

            var client = new HttpClient(
                new SocketsHttpHandler()
                {
                    ConnectCallback = (SocketsHttpConnectionContext context, CancellationToken cancellationToken) =>
                    {
                        if (context.DnsEndPoint.Port != port
                            || context.DnsEndPoint.Host != pod.Metadata.Name)
                        {
                            throw new InvalidOperationException($"This HttpClient only supports connecting to port {port} on pod {pod.Metadata.Name}");
                        }

                        return this.ConnectToPodPortAsync(pod, port, cancellationToken);
                    },
                });

            client.BaseAddress = new Uri($"http://{pod.Metadata.Name}:{port}/");
            return client;
        }

        /// <summary>
        /// Connects to a TCP port exposed by a pod.
        /// </summary>
        /// <param name="pod">
        /// The pod to which to connect.
        /// </param>
        /// <param name="port">
        /// The TCP port number of the port to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns
        /// a <see cref="Stream"/> which can be used to send and receive data to the remote
        /// port.
        /// </returns>
        public virtual async ValueTask<Stream> ConnectToPodPortAsync(V1Pod pod, int port, CancellationToken cancellationToken)
        {
            EnsureObjectMetadata(pod);

            this.logger.LogInformation("Connecting to port {port} on pod {pod}", pod, pod.Metadata.Name);

            var webSocket = await this.protocol.WebSocketNamespacedPodPortForwardAsync(
                pod.Metadata.Name,
                pod.Metadata.NamespaceProperty,
                new int[] { port },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Connected to port {port} on pod {pod}", port, pod.Metadata.Name);
            return new PortForwardStream(webSocket, this.loggerFactory.CreateLogger<PortForwardStream>());
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
        public virtual Task DeletePodAsync(V1Pod value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.DeleteNamespacedObjectAsync<V1Pod>(
                value,
                this.protocol.DeleteNamespacedPodAsync,
                this.WatchPodAsync,
                timeout,
                cancellationToken);
        }

        private static bool IsRunning(V1Pod value)
        {
            return value.Status.Phase == "Running"
                && value.Status.ContainerStatuses.All(c => c.Ready);
        }

        private static bool HasFailed(V1Pod value, [NotNullWhen(true)] out Exception? ex)
        {
            if (value.Status.Phase == "Failed")
            {
                ex = new KubernetesException($"The pod {value.Metadata.Name} has failed: {value.Status.Reason}");
                return true;
            }
            else
            {
                ex = null;
                return false;
            }
        }

        /// <summary>
        /// Validates the <see cref="V1Pod"/> object by ensuring it is not <see langword="null"/>,
        /// and its metadata contain at least the <see cref="V1ObjectMeta.Name"/> and <see cref="V1ObjectMeta.NamespaceProperty"/>.
        /// </summary>
        /// <param name="pod">
        /// The pod to validate.
        /// </param>
        private static void EnsureObjectMetadata(V1Pod pod)
        {
            if (pod == null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            if (pod.Metadata == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "pod.Metadata");
            }

            if (pod.Metadata.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "pod.Metadata.Name");
            }

            if (pod.Metadata.NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "pod.Metadata.NamespaceProperty");
            }
        }
    }
}
