// <copyright file="ConnectionState.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Defines the state of an Android device connected to the <c>ADB</c> server.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/adb/+/refs/heads/master/adb.h"/>
    /// <seealso href="https://android.googlesource.com/platform/system/adb/+/refs/heads/master/transport.cpp"/>
    public enum ConnectionState
    {
        /// <summary>
        /// The device state is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The device is in offline mode.
        /// </summary>
        Offline,

        /// <summary>
        /// The device is in bootloader mode.
        /// </summary>
        Bootloader,

        /// <summary>
        /// The device is in device mode.
        /// </summary>
        Device,

        /// <summary>
        /// The device is the adb host.
        /// </summary>
        Host,

        /// <summary>
        /// The device is in recovery mode.
        /// </summary>
        Recovery,

        /// <summary>
        /// Insufficient permissions to communicate with the device.
        /// </summary>
        NoPermissions,

        /// <summary>
        /// The device is in sideload mode.
        /// </summary>
        Sideload,

        /// <summary>
        /// The device is in unauthorized mode.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// The device is in authorizing mode.
        /// </summary>
        Authorizing,
    }
}
