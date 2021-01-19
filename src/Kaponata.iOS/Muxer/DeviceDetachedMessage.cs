// <copyright file="DeviceDetachedMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.Muxer
{
    /// <summary>
    /// Represents the message which is sent by <c>usbmuxd</c> when a device is detached (disconnected)
    /// from the host.
    /// </summary>
    public partial class DeviceDetachedMessage : DeviceMessage
    {
    }
}
