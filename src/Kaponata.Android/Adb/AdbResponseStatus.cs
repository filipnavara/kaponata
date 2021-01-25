// <copyright file="AdbResponseStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents the status values returned by the <c>ADB</c> server.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/core/+/android-p-preview-4/adb/adb_io.cpp"/>
    public enum AdbResponseStatus : int
    {
        /// <summary>
        /// The OKAY message send by the <c>ADB</c> server.
        /// </summary>
        OKAY = 0x4f4b4159,

        /// <summary>
        /// The FAIL message send by the <c>ADB</c> server.
        /// </summary>
        FAIL = 0x4641494c,
    }
}
