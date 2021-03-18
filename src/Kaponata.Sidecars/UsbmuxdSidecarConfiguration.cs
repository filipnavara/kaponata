// <copyright file="UsbmuxdSidecarConfiguration.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Sidecars
{
    /// <summary>
    /// Configuration for the <see cref="UsbmuxdSidecar"/>.
    /// </summary>
    public class UsbmuxdSidecarConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the pod which hosts the iOS USB multiplexer, and for which this sidecar
        /// is running.
        /// </summary>
        public string PodName { get; set; }
    }
}
