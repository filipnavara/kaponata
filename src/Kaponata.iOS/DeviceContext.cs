// <copyright file="DeviceContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;

namespace Kaponata.iOS
{
    /// <summary>
    /// The <see cref="DeviceContext"/> represents a single connection to an iOS device.
    /// It can be used in session scopes to inject information about the current iOS device
    /// to via the constructor.
    /// </summary>
    public class DeviceContext
    {
        /// <summary>
        /// Gets or sets the device to which the session is scoped.
        /// </summary>
        public MuxerDevice Device { get; set; }
    }
}
