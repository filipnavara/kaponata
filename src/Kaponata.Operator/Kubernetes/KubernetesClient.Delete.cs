// <copyright file="KubernetesClient.Delete.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Rest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Contains generic methods for deleting Kubernetes objects.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <summary>
        /// A delegate for operations which delete namespaced Kubernetes objects.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the Kubernetes object to delete.
        /// </typeparam>
        /// <param name="name">
        /// The name of the object to delete.
        /// </param>
        /// <param name="namespaceParameter">
        /// The namespace in which to delete the object.
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
        public delegate Task<T> DeleteNamespacedObjectAsyncDelegate<T>(
            string name,
            string namespaceParameter,
            V1DeleteOptions body = null,
            string dryRun = null,
            int? gracePeriodSeconds = null,
            bool? orphanDependents = null,
            string propagationPolicy = null,
            string pretty = null,
            CancellationToken cancellationToken = default)
            where T : IKubernetesObject<V1ObjectMeta>;

        private delegate Task<V1Status> DeleteObjectAsyncDelegate(
            string name,
            V1DeleteOptions body = null,
            string dryRun = null,
            int? gracePeriodSeconds = null,
            bool? orphanDependents = null,
            string propagationPolicy = null,
            string pretty = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously deletes a namespaced object, and waits for the delete operation
        /// to complete.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the Kubernetes object to delete.
        /// </typeparam>
        /// <param name="value">
        /// The object to delete.
        /// </param>
        /// <param name="deleteAction">
        /// A delegate to a method which schedules the deletion.
        /// </param>
        /// <param name="watchAction">
        /// A delegate to a method which creates a watcher for the object.
        /// Used to monitor the progress of the delete operation.
        /// </param>
        /// <param name="timeout">
        /// The amount of time to wait for the object to be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the
        /// asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task DeleteNamespacedObjectAsync<T>(
            T value,
            DeleteNamespacedObjectAsyncDelegate<T> deleteAction,
            WatchObjectAsyncDelegate<T> watchAction,
            TimeSpan timeout,
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

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = watchAction(
                value,
                onEvent: (type, updatedValue) =>
                {
                    value = updatedValue;

                    if (type == WatchEventType.Deleted)
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            await deleteAction(
                value.Metadata.Name,
                value.Metadata.NamespaceProperty,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The {value.Kind} '{value.Metadata.Name}' was not deleted within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching {value.Kind} '{value.Metadata.Name}'.");
            }
        }

        private async Task DeleteObjectAsync<T>(
            T value,
            DeleteObjectAsyncDelegate deleteAction,
            WatchObjectAsyncDelegate<T> watchAction,
            TimeSpan timeout,
            CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = watchAction(
                value,
                onEvent: (type, updatedValue) =>
                {
                    value = updatedValue;

                    if (type == WatchEventType.Deleted)
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            await deleteAction(
                value.Metadata.Name,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The {value.Kind} '{value.Metadata.Name}' was not deleted within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching {value.Kind} '{value.Metadata.Name}'.");
            }
        }
    }
}
