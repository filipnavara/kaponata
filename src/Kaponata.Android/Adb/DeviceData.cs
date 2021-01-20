// <copyright file="DeviceData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents a device that is connected to the Android Debug Bridge.
    /// </summary>
    public partial class DeviceData
    {
        /// <summary>
        /// Gets or sets the device serial number.
        /// </summary>
        public string Serial
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public ConnectionState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string Features
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the USB port to which this device is connected.
        /// </summary>
        public string Usb
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device info message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Serial;
        }
    }
}
