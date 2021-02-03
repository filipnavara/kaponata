// <copyright file="KubernetesClientMockExtensions.cs" company="Quamotion bv">
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
    /// Provides extension methods for <see cref="KubernetesClient"/> mocks.
    /// </summary>
    internal static class KubernetesClientMockExtensions
    {
        /// <summary>
        /// Mocks the value of the <see cref="KubernetesClient.ListPodAsync(string, string, string, string, int?, CancellationToken)"/> method.
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
                .Setup(k => k.ListPodAsync("default", null, null, labelSelector, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new V1PodList()
                    {
                        Items = items,
                    }));

            return items;
        }

        /// <summary>
        /// Mocks the value of the <see cref="KubernetesClient.ListMobileDeviceAsync(string, string, string, string, int?, CancellationToken)"/> method.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector which will be used to list the devices.
        /// </param>
        /// <param name="devices">
        /// The devices which should be returned.
        /// </param>
        /// <returns>
        /// A result which represents the mock configuration.
        /// </returns>
        public static List<MobileDevice> WithDeviceList(this Mock<KubernetesClient> client, string labelSelector, params MobileDevice[] devices)
        {
            var items = new List<MobileDevice>(devices);

            client
                .Setup(k => k.ListMobileDeviceAsync("default", null, null, labelSelector, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<ItemList<MobileDevice>>(
                    new MobileDeviceList()
                    {
                        Items = items,
                    }));

            return items;
        }

        /// <summary>
        /// Configures the <see cref="KubernetesClient.CreateMobileDeviceAsync(MobileDevice, CancellationToken)"/> method on the mock,
        /// and tracks the newly created devices.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <returns>
        /// A collection to which newly created devices will be added.
        /// </returns>
        public static Collection<MobileDevice> TrackCreatedDevices(this Mock<KubernetesClient> client)
        {
            var devices = new Collection<MobileDevice>();

            client
                .Setup(d => d.CreateMobileDeviceAsync(It.IsAny<MobileDevice>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<MobileDevice, CancellationToken, KubernetesClient, MobileDevice>(
                (device, cancellationToken) =>
                {
                    devices.Add(device);
                    return device;
                });

            return devices;
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
        /// Configures the <see cref="KubernetesClient.DeleteMobileDeviceAsync(MobileDevice, TimeSpan, CancellationToken)"/> method on the mock,
        /// and tracks the deleted devices.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <returns>
        /// A collection to which deleted devices will be added.
        /// </returns>
        public static Collection<MobileDevice> TrackDeletedDevices(this Mock<KubernetesClient> client)
        {
            var devices = new Collection<MobileDevice>();

            client
                .Setup(d => d.DeleteMobileDeviceAsync(It.IsAny<MobileDevice>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<MobileDevice, TimeSpan, CancellationToken>(
                (device, timeout, cancellationToken) =>
                {
                    devices.Add(device);
                })
                .Returns(Task.CompletedTask);

            return devices;
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
                    "default",
                    null /* fieldSelector */,
                    labelSelector /* labelSelector */,
                    null /* resourceVersion */,
                    It.IsAny<WatchEventDelegate<V1Pod>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, string, string, WatchEventDelegate<V1Pod>, CancellationToken>(
                (@namespace, fieldSelector, labelSelector, resourceVersion, eventHandler, cancellationToken) =>
                {
                    cancellationToken.Register(watchClient.TaskCompletionSource.SetCanceled);
                    watchClient.ClientRegistered.SetResult(eventHandler);
                })
                .Returns(watchClient.TaskCompletionSource.Task);

            return watchClient;
        }

        /// <summary>
        /// Configures the <see cref="KubernetesClient.WatchMobileDeviceAsync(string, string, string, string, WatchEventDelegate{MobileDevice}, CancellationToken)"/> method
        /// on the mock.
        /// </summary>
        /// <param name="client">
        /// The mock to configure.
        /// </param>
        /// <param name="labelSelector">
        /// The label selector used to watch the devices.
        /// </param>
        /// <returns>
        /// A <see cref="WatchClient{T}"/> which can be used to invoke the event handler (if set) and complete the watch operation.
        /// </returns>
        public static WatchClient<MobileDevice> WithDeviceWatcher(this Mock<KubernetesClient> client, string labelSelector)
        {
            var watchClient = new WatchClient<MobileDevice>();

            client
                .Setup(k => k.WatchMobileDeviceAsync(
                    "default",
                    null /* fieldSelector */,
                    labelSelector, /* labelSelector */
                    null /* resourceVersion */,
                    It.IsAny<WatchEventDelegate<MobileDevice>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, string, string, WatchEventDelegate<MobileDevice>, CancellationToken>(
                (@namespace, fieldSelector, labelSelector, resourceVersion, eventHandler, cancellationToken) =>
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
