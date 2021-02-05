// <copyright file="AdbListenAction.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Defines whether a listener should continue listening or stop listening for device connect or disconnect events.
    /// </summary>
    public enum AdbListenAction : byte
    {
        /// <summary>
        /// The listener should keep listening for new device connect or disconnect events.
        /// </summary>
        ContinueListening = 1,

        /// <summary>
        /// The listener should stop listening for device connect or disconnect events.
        /// </summary>
        StopListening = 2,
    }
}
