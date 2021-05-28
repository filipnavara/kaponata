// <copyright file="MobileDeviceConditions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// Contains the names of well-known device conditions.
    /// </summary>
    public class MobileDeviceConditions
    {
        /// <summary>
        /// The name of the condition which indicates whether the device is ready.
        /// </summary>
        public const string Ready = nameof(Ready);

        /// <summary>
        /// The name of the condition which indicates whether the device has paired successfully with the host.
        /// </summary>
        public const string Paired = nameof(Paired);

        /// <summary>
        /// The name of the condition which indicates whether the developer disk has mounted successfully on the devie.
        /// </summary>
        public const string DeveloperDiskMounted = nameof(DeveloperDiskMounted);
    }
}
