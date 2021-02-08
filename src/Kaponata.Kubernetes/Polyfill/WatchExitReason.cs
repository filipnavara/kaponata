// <copyright file="WatchExitReason.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Kubernetes.Polyfill
{
    /// <summary>
    /// Describes the reason why a watch stopped.
    /// </summary>
    public enum WatchExitReason
    {
        /// <summary>
        /// The client disconnected.
        /// </summary>
        ClientDisconnected = 1,

        /// <summary>
        /// The server disconnected.
        /// </summary>
        ServerDisconnected = 2,
    }
}
