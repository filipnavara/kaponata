// <copyright file="DeviceInfo.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Represents the device info as transferred by ScrCpy.
    /// </summary>
    public partial struct DeviceInfo
    {
        /// <summary>
        /// The device name.
        /// </summary>
        public string DeviceName;

        /// <summary>
        /// The width of the device.
        /// </summary>
        public ushort Width;

        /// <summary>
        /// The height of the device.
        /// </summary>
        public ushort Height;
    }
}
