// <copyright file="NamespacedKubernetesClientExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Extensions for mocks of the <see cref="NamespacedKubernetesClient{T}"/> class.
    /// </summary>
    public static class NamespacedKubernetesClientExtensions
    {
        /// <summary>
        /// Mocks the value of the <see cref="NamespacedKubernetesClient{T}.ListAsync(string, string, string, string, int?, CancellationToken)"/> method.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="fieldSelector">
        /// The field selector used to list the items.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector which will be used to list the items.
        /// </param>
        /// <param name="values">
        /// The pods which should be returned.
        /// </param>
        /// <typeparam name="T">
        /// The type of objects observed by the client.
        /// </typeparam>
        /// <returns>
        /// The list of pods which will be returned to the client.
        /// </returns>
        public static List<T> WithList<T>(this Mock<NamespacedKubernetesClient<T>> client, string fieldSelector, string labelSelector, params T[] values)
            where T : IKubernetesObject<V1ObjectMeta>, new()
        {
            var items = new List<T>(values);

            client
                .Setup(k => k.ListAsync("default", null, fieldSelector, labelSelector, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new ItemList<T>()
                    {
                        Items = items,
                    }));

            return items;
        }

        /// <summary>
        /// Configures the <see cref="NamespacedKubernetesClient{T}.CreateAsync(T, CancellationToken)"/> method on the mock,
        /// and tracks the newly created devices.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <typeparam name="T">
        /// The type of objects observed by the client.
        /// </typeparam>
        /// <returns>
        /// A collection to which newly created devices will be added.
        /// </returns>
        public static (Collection<T> items, Task<T> firstItemCreated) TrackCreatedItems<T>(this Mock<NamespacedKubernetesClient<T>> client)
            where T : IKubernetesObject<V1ObjectMeta>, new()
        {
            var items = new Collection<T>();
            var firstChildCreated = new TaskCompletionSource<T>();

            client
                .Setup(d => d.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<T, CancellationToken, NamespacedKubernetesClient<T>, T>(
                (item, cancellationToken) =>
                {
                    items.Add(item);
                    firstChildCreated.TrySetResult(item);
                    return item;
                });

            return (items, firstChildCreated.Task);
        }

        /// <summary>
        /// Configures the <see cref="NamespacedKubernetesClient{T}.DeleteAsync(T, TimeSpan, CancellationToken)"/> method on the mock,
        /// and tracks the deleted devices.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <typeparam name="T">
        /// The type of objects observed by the client.
        /// </typeparam>
        /// <returns>
        /// A collection to which deleted devices will be added.
        /// </returns>
        public static Collection<MobileDevice> TrackDeletedItems<T>(this Mock<NamespacedKubernetesClient<T>> client)
            where T : IKubernetesObject<V1ObjectMeta>, new()
        {
            var devices = new Collection<MobileDevice>();

            client
                .Setup(d => d.DeleteAsync(It.IsAny<T>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<MobileDevice, TimeSpan, CancellationToken>(
                (device, timeout, cancellationToken) =>
                {
                    devices.Add(device);
                })
                .Returns(Task.CompletedTask);

            return devices;
        }

        /// <summary>
        /// Configures the <see cref="NamespacedKubernetesClient{T}.WatchAsync(string, string, string, string, WatchEventDelegate{T}, CancellationToken)"/> method
        /// on the mock.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="fieldSelector">
        /// The field selector used to watch th eitems.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector used to watch the items.
        /// </param>
        /// <typeparam name="T">
        /// The type of objects observed by the client.
        /// </typeparam>
        /// <returns>
        /// A <see cref="WatchClient{T}"/> which can be used to invoke the event handler (if set) and complete the watch operation.
        /// </returns>
        public static WatchClient<T> WithWatcher<T>(this Mock<NamespacedKubernetesClient<T>> client, string fieldSelector, string labelSelector)
            where T : IKubernetesObject<V1ObjectMeta>, new()
        {
            var watchClient = new WatchClient<T>();

            client
                .Setup(k => k.WatchAsync(
                    "default",
                    fieldSelector /* fieldSelector */,
                    labelSelector /* labelSelector */,
                    null /* resourceVersion */,
                    It.IsAny<WatchEventDelegate<T>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, string, string, WatchEventDelegate<T>, CancellationToken>(
                (@namespace, fieldSelector, labelSelector, resourceVersion, eventHandler, cancellationToken) =>
                {
                    cancellationToken.Register(watchClient.TaskCompletionSource.SetCanceled);
                    watchClient.ClientRegistered.SetResult(eventHandler);
                })
                .Returns(watchClient.TaskCompletionSource.Task);

            return watchClient;
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
