// <copyright file="ConditionStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// Describes the status of a individual condition.
    /// </summary>
    public enum ConditionStatus
    {
        /// <summary>
        /// The condition is met.
        /// </summary>
        True,

        /// <summary>
        /// The condition is not met.
        /// </summary>
        False,

        /// <summary>
        /// The status of the condition is unknown.
        /// </summary>
        Unknown,
    }
}
