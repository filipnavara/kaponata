// <copyright file="ForwardData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains information about port forwarding configured by the Android Debug Bridge.
    /// </summary>
    public partial class ForwardData
    {
        /// <summary>
        /// Gets or sets the serial number of the device for which the port forwarding is
        /// configured.
        /// </summary>
        public string SerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a <see cref="ForwardSpec"/> that represents the local (PC) endpoint.
        /// </summary>
        public ForwardSpec LocalSpec
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a <see cref="ForwardSpec"/> that represents the remote (device) endpoint.
        /// </summary>
        public ForwardSpec RemoteSpec
        {
            get;
            set;
        }
    }
}
