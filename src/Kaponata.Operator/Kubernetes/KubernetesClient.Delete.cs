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
        private delegate Task<T> DeleteNamespacedObjectAsyncDelegate<T>(
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

        private async Task DeleteNamespacedObjectAsync<T>(
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

            if (value.Metadata?.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.Name");
            }

            if (value.Metadata?.NamespaceProperty == null)
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
