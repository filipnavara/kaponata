// <copyright file="KubernetesClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Performs high-level transactions (such as creating a Pod, waiting for a pod to enter a specific state,...)
    /// on top of the <see cref="IKubernetesProtocol"/>.
    /// </summary>
    public class KubernetesClient : IDisposable
    {
        private readonly IKubernetesProtocol protocol;
        private readonly ILogger<KubernetesClient> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// The protocol to use to communicate with the Kubernetes API server.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public KubernetesClient(IKubernetesProtocol protocol, ILogger<KubernetesClient> logger)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously waits for a pod to enter the running phase.
        /// </summary>
        /// <param name="pod">
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
        public virtual async Task<V1Pod> WaitForPodRunningAsync(V1Pod pod, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (pod == null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            if (IsRunning(pod))
            {
                return pod;
            }

            if (HasFailed(pod, out Exception ex))
            {
                throw ex;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = this.protocol.WatchPodAsync(
                pod,
                onEvent: (type, updatedPod) =>
                {
                    pod = updatedPod;

                    if (type == WatchEventType.Deleted)
                    {
                        throw new KubernetesException($"The pod {pod.Metadata.Name} was deleted.");
                    }

                    // Wait for the pod to be ready. Since we work with init containers, merely waiting for the pod to have an IP address is not enough -
                    // we need both 'Running' status _and_ and IP address
                    if (IsRunning(pod))
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    if (HasFailed(pod, out Exception ex))
                    {
                        throw ex;
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The pod {pod.Metadata.Name} did not transition to the completed state within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching pod {pod.Metadata.Name}.");
            }

            return pod;
        }

        /// <summary>
        /// Asynchronously deletes a pod.
        /// </summary>
        /// <param name="pod">
        /// The pod to delete.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the pod should be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeletePodAsync(V1Pod pod, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (pod == null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = this.protocol.WatchPodAsync(
                pod,
                onEvent: (type, updatedPod) =>
                {
                    pod = updatedPod;

                    if (type == WatchEventType.Deleted)
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            await this.protocol.DeleteNamespacedPodAsync(
                pod.Metadata.Name,
                pod.Metadata.NamespaceProperty,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The pod {pod.Metadata.Name} was not deleted within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching pod {pod.Metadata.Name}.");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.protocol.Dispose();
        }

        private static bool IsRunning(V1Pod pod)
        {
            return pod.Status.Phase == "Running";
        }

        private static bool HasFailed(V1Pod pod, out Exception ex)
        {
            if (pod.Status.Phase == "Failed")
            {
                ex = new KubernetesException($"The pod {pod.Metadata.Name} has failed: {pod.Status.Reason}");
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
