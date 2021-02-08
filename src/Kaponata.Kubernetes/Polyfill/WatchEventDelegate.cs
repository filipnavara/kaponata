// <copyright file="WatchEventDelegate.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// The definition of a callback which is invoked when a the Kubernetes API server sends a watch event.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object being watched.
    /// </typeparam>
    /// <param name="eventType">
    /// The watch event which has just occurred.
    /// </param>
    /// <param name="value">
    /// The object which has changed.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> which represents the asynchronous operation during which the client
    /// processes the watch event, and which returns a value indicating whether the watch loop
    /// should to continue watching for events or should close the connection with the server.
    /// </returns>
    public delegate Task<WatchResult> WatchEventDelegate<T>(WatchEventType eventType, T value)
        where T : IKubernetesObject<V1ObjectMeta>;
}
