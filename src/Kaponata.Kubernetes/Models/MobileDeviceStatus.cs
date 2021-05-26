// <copyright file="MobileDeviceStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// Describes the status of a <see cref="MobileDevice"/> object.
    /// </summary>
    public class MobileDeviceStatus
    {
        /// <summary>
        /// Gets or sets, when available, the name of a service or IP address of a pod which hosts a VNC server
        /// which allows users to remotely control this device.
        /// </summary>
        [JsonProperty(PropertyName = "vncHost")]
        public string VncHost { get; set; }

        /// <summary>
        /// Gets or sets, when available, a password which can be used to connect to the VNC server at
        /// <see cref="VncHost"/>.
        /// </summary>
        [JsonProperty(PropertyName = "vncPassword")]
        public string VncPassword { get; set; }

        /// <summary>
        /// Gets or sets a list of all conditions currently known for this device.
        /// </summary>
        [JsonProperty(PropertyName = "conditions")]
        public IList<MobileDeviceCondition> Conditions { get; set; }

        /// <summary>
        /// Updates a condition.
        /// </summary>
        /// <param name="type">
        /// The name of the condition to update.
        /// </param>
        /// <param name="status">
        /// The current status of the condition.
        /// </param>
        /// <param name="reason">
        /// A machine-readable status code which describes the reason for the condition.
        /// </param>
        /// <param name="message">
        /// A human-readable message which describes the reason for the condition.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the condition has been updated; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool UpdateCondition(string type, ConditionStatus status, string reason, string message)
        {
            var currentCondition = this.Conditions.SingleOrDefault(c => c.Type == type);

            if (currentCondition == null)
            {
                this.Conditions.Add(
                    new MobileDeviceCondition()
                    {
                        Type = type,
                        Status = status,
                        Reason = reason,
                        Message = message,
                        LastHeartbeatTime = DateTimeOffset.Now,
                        LastTransitionTime = DateTimeOffset.Now,
                    });

                return true;
            }
            else if (currentCondition.Status != status)
            {
                currentCondition.LastHeartbeatTime = DateTimeOffset.Now;
                currentCondition.LastTransitionTime = DateTimeOffset.Now;
                currentCondition.Status = status;
                currentCondition.Reason = reason;
                currentCondition.Message = message;

                return true;
            }
            else if (currentCondition.LastHeartbeatTime.AddMinutes(5) > DateTimeOffset.Now)
            {
                currentCondition.LastHeartbeatTime = DateTimeOffset.Now;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
