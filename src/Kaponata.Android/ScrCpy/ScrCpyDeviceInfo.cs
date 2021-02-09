// <copyright file="ScrCpyDeviceInfo.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Represents the device info as transferred by ScrCpy.
    /// </summary>
    public partial class ScrCpyDeviceInfo
    {
        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string DeviceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width of the device.
        /// </summary>
        public ushort Width
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the device.
        /// </summary>
        public ushort Height
        {
            get;
            set;
        }
    }
}
