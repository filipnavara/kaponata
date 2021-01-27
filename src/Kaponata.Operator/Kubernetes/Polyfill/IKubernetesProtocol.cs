// <copyright file="IKubernetesProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// Extends the <see cref="k8s.IKubernetes"/> client with polyfill methods.
    /// </summary>
    public interface IKubernetesProtocol : IKubernetes
    {
        /// <summary>
        /// Gets the <see cref="HttpClient"/> used to send HTTP requests.
        /// </summary>
        HttpClient HttpClient { get; }

        /// <summary>
        /// Asynchronously watches a custom resource definition.
        /// </summary>
        /// <param name="crd">
        /// The custom resource definition being watched.
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
        Task<WatchExitReason> WatchCustomResourceDefinitionAsync(
            V1CustomResourceDefinition crd,
            WatchEventDelegate<V1CustomResourceDefinition> eventHandler,
            CancellationToken cancellationToken);

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
        Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            TObject value,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>;

        /// <summary>
        /// Asynchronously watches namespaced objects.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to watch.
        /// </typeparam>
        /// <typeparam name="TList">
        /// The type of a list of <typeparamref name="TObject"/> objects.
        /// </typeparam>
        /// <param name="namespace">
        /// The namespace in which to watch for <typeparamref name="TObject"/> objects.
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
        Task<WatchExitReason> WatchNamespacedObjectAsync<TObject, TList>(
            string @namespace,
            string fieldSelector,
            string labelSelector,
            string resourceVersion,
            ListNamespacedObjectWithHttpMessagesAsync<TObject, TList> listOperation,
            WatchEventDelegate<TObject> eventHandler,
            CancellationToken cancellationToken)
            where TObject : IKubernetesObject<V1ObjectMeta>
            where TList : IItems<TObject>;
    }
}
