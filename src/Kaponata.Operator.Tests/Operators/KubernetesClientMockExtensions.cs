// <copyright file="KubernetesClientMockExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Provides extension methods for <see cref="KubernetesClient"/> mocks.
    /// </summary>
    internal static class KubernetesClientMockExtensions
    {
        /// <summary>
        /// Mocks the value of the <see cref="KubernetesClient.ListPodAsync(string, string, string, int?, CancellationToken)"/> method.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector which will be used to list the pods.
        /// </param>
        /// <param name="pods">
        /// The pods which should be returned.
        /// </param>
        /// <returns>
        /// The list of pods which will be returned to the client.
        /// </returns>
        public static List<V1Pod> WithPodList(this Mock<KubernetesClient> client, string labelSelector, params V1Pod[] pods)
        {
            var items = new List<V1Pod>(pods);

            client
                .Setup(k => k.ListPodAsync(null, null, labelSelector, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new V1PodList()
                    {
                        Items = items,
                    }));

            return items;
        }

        /// <summary>
        /// Configures the <see cref="KubernetesClient.CreatePodAsync(V1Pod, CancellationToken)"/> method on the mock,
        /// and tracks the newly created pods.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <returns>
        /// A collection to which newly created pods will be added.
        /// </returns>
        public static Collection<V1Pod> TrackCreatedPods(this Mock<KubernetesClient> client)
        {
            var pods = new Collection<V1Pod>();

            client
                .Setup(d => d.CreatePodAsync(It.IsAny<V1Pod>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<V1Pod, CancellationToken, KubernetesClient, V1Pod>(
                (pod, cancellationToken) =>
                {
                    pods.Add(pod);
                    return pod;
                });

            return pods;
        }

        /// <summary>
        /// Configures the <see cref="KubernetesClient.DeletePodAsync(V1Pod, TimeSpan, CancellationToken)"/> method on the mock,
        /// and tracks the deleted pods.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <returns>
        /// A collection to which deleted pods will be added.
        /// </returns>
        public static Collection<V1Pod> TrackDeletedPods(this Mock<KubernetesClient> client)
        {
            var pods = new Collection<V1Pod>();

            client
                .Setup(d => d.DeletePodAsync(It.IsAny<V1Pod>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<V1Pod, TimeSpan, CancellationToken>(
                (pod, timeout, cancellationToken) =>
                {
                    pods.Add(pod);
                })
                .Returns(Task.CompletedTask);

            return pods;
        }

        /// <summary>
        /// Configures the <see cref="KubernetesClient.WatchPodAsync(string, string, string, string, WatchEventDelegate{V1Pod}, CancellationToken)"/> method
        /// on the mock.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector used to watch the pods.
        /// </param>
        /// <returns>
        /// A <see cref="WatchClient{T}"/> which can be used to invoke the event handler (if set) and complete the watch operation.
        /// </returns>
        public static WatchClient<V1Pod> WithPodWatcher(this Mock<KubernetesClient> client, string labelSelector)
        {
            var watchClient = new WatchClient<V1Pod>();

            client
                .Setup(k => k.WatchPodAsync(
                    null /* fieldSelector */,
                    labelSelector /* labelSelector */,
                    null /* resourceVersion */,
                    null /* resourceVersionMatch */,
                    It.IsAny<WatchEventDelegate<V1Pod>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, string, WatchEventDelegate<V1Pod>, CancellationToken>(
                (fieldSelector, labelSelector, resourceVersion, eventHandler, cancellationToken) =>
                {
                    cancellationToken.Register(watchClient.TaskCompletionSource.SetCanceled);
                    watchClient.ClientRegistered.SetResult(eventHandler);
                })
                .Returns(watchClient.TaskCompletionSource.Task);

            return watchClient;
        }

        /// <summary>
        /// Sets up a <see cref="NamespacedKubernetesClient{T}"/> which is a child of this <see cref="KubernetesClient"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of objects observed by the <see cref="NamespacedKubernetesClient{T}"/>.
        /// </typeparam>
        /// <param name="client">
        /// The <see cref="KubernetesClient"/> for which to configure the <see cref="NamespacedKubernetesClient{T}"/>.
        /// </param>
        /// <returns>
        /// A mock of the <see cref="NamespacedKubernetesClient{T}"/> class.
        /// </returns>
        public static Mock<NamespacedKubernetesClient<T>> WithClient<T>(this Mock<KubernetesClient> client)
            where T : class, IKubernetesObject<V1ObjectMeta>, new()
        {
            Mock<NamespacedKubernetesClient<T>> typedClient = new Mock<NamespacedKubernetesClient<T>>(MockBehavior.Strict);

            client.Setup(s => s.GetClient<T>())
                .Returns(typedClient.Object);

            return typedClient;
        }

        /// <summary>
        /// A client which subscribed to a watch operation.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object being watched.
        /// </typeparam>
        public class WatchClient<T>
            where T : IKubernetesObject<V1ObjectMeta>
        {
            /// <summary>
            /// Gets a <see cref="TaskCompletionSource"/> which returns the first client which registered.
            /// </summary>
            public TaskCompletionSource<WatchEventDelegate<T>> ClientRegistered { get; } = new TaskCompletionSource<WatchEventDelegate<T>>();

            /// <summary>
            /// Gets a <see cref="TaskCompletionSource"/> which can be used to complete the watch task.
            /// </summary>
            public TaskCompletionSource<WatchExitReason> TaskCompletionSource { get; } = new TaskCompletionSource<WatchExitReason>();
        }
    }
}
