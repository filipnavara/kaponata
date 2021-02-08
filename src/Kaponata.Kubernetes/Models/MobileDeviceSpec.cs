// <copyright file="MobileDeviceSpec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// Defines the specifications (invariant data) related to a <see cref="MobileDevice"/> object.
    /// </summary>
    public class MobileDeviceSpec
    {
        /// <summary>
        /// Gets or sets the name of the pod which owns this device. This is usually the pod which hosts the
        /// usbmuxd or adb instance to which the device is connected.
        /// </summary>
        [JsonProperty("owner")]
        public string Owner { get; set; }
    }
}
