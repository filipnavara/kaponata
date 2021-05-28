// <copyright file="MobileDeviceCondition.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

#nullable disable

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// <see cref="MobileDeviceCondition"/> contains details for the current condition of this <see cref="MobileDevice"/>.
    /// </summary>
    /// <seealso href="https://github.com/kubernetes/community/blob/master/contributors/devel/sig-architecture/api-conventions.md#typical-status-properties"/>
    public class MobileDeviceCondition
    {
        /// <summary>
        /// Gets or sets the type of the condition. The condition type uniquely identifies the condition.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the condition.
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a (brief) reason for the condition's last transition.
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets human readable message indicating details about last transition.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the last time the condition was probed.
        /// </summary>
        [JsonProperty("lastHeartbeatTime")]
        public DateTimeOffset LastHeartbeatTime { get; set; }

        /// <summary>
        /// Gets or sets the last time the device condition transitioned from one status to another.
        /// </summary>
        [JsonProperty("lastTransitionTime")]
        public DateTimeOffset LastTransitionTime { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Type}: {this.Status} ({this.Reason})";
        }
    }
}
