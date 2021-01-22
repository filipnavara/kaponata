// <copyright file="KubernetesClient.Pods.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Rest;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
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
        public virtual async Task<V1Pod> TryReadPodAsync(string @namespace, string name, CancellationToken cancellationToken)
        {
            var list = await this.RunTaskAsync(this.protocol.ListNamespacedPodAsync(@namespace, fieldSelector: $"metadata.name={name}", cancellationToken: cancellationToken)).ConfigureAwait(false);
            return list.Items.SingleOrDefault();
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

            if (HasFailed(value, out Exception ex))
            {
                throw ex;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = this.protocol.WatchPodAsync(
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

                    if (HasFailed(value, out Exception ex))
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
                this.protocol.WatchPodAsync,
                timeout,
                cancellationToken);
        }

        private static bool IsRunning(V1Pod value)
        {
            return value.Status.Phase == "Running";
        }

        private static bool HasFailed(V1Pod value, out Exception ex)
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
    }
}
