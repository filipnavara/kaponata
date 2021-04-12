// <copyright file="Notifications.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.NotificationProxy
{
    /// <summary>
    /// Enumerates notifications which can be handled by the <see cref="NotificationProxyClient"/>.
    /// </summary>
    public static class Notifications
    {
        /// <summary>
        /// The user has accepted a pairing request.
        /// </summary>
        public const string RequestPair = "com.apple.mobile.lockdown.request_pair";

        /// <summary>
        /// The device requests the BUID of the host device.
        /// </summary>
        public const string RequestHostBuid = "com.apple.mobile.lockdown.request_host_buid";
    }
}
