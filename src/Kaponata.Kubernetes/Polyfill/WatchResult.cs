// <copyright file="WatchResult.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Kubernetes.Polyfill
{
    /// <summary>
    /// Defines the action the watcher should take after an event has been received
    /// and processed by the client.
    /// </summary>
    public enum WatchResult
    {
        /// <summary>
        /// Continue watching for new events.
        /// </summary>
        Continue = 1,

        /// <summary>
        /// Stop watching.
        /// </summary>
        Stop = 2,
    }
}
