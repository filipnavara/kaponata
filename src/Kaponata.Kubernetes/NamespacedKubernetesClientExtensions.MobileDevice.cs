// <copyright file="NamespacedKubernetesClientExtensions.MobileDevice.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Models;
using Microsoft;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Provides extension methods for the <see cref="NamespacedKubernetesClient{T}"/> class.
    /// </summary>
    public static class NamespacedKubernetesClientExtensions
    {
        /// <summary>
        /// Asynchronously sets a device condition.
        /// </summary>
        /// <param name="client">
        /// A client which provides access to the Kubernetes cluster.
        /// </param>
        /// <param name="device">
        /// The device for which to set the status.
        /// </param>
        /// <param name="type">
        /// The name of the condition.
        /// </param>
        /// <param name="status">
        /// The status of the condition.
        /// </param>
        /// <param name="reason">
        /// A machine-readable reason for the condition status.
        /// </param>
        /// <param name="message">
        /// A human-readable reason for the condition status.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public static async Task SetDeviceConditionAsync(this NamespacedKubernetesClient<MobileDevice> client, MobileDevice device, string type, ConditionStatus status, string reason, string message, CancellationToken cancellationToken)
        {
            Requires.NotNull(device, nameof(device));
            Requires.NotNull(type, nameof(type));

            bool hasStatus = device.Status != null;
            bool hasConditions = device.Status?.Conditions != null;

            if (device.Status == null)
            {
                device.Status = new MobileDeviceStatus();
            }

            if (device.Status.SetCondition(type, status, reason, message))
            {
                var patch = new JsonPatchDocument<MobileDevice>();

                if (!hasStatus)
                {
                    patch.Add(p => p.Status, device.Status);
                }
                else if (!hasConditions)
                {
                    patch.Add(p => p.Status.Conditions, device.Status.Conditions);
                }
                else
                {
                    patch.Replace(p => p.Status.Conditions, device.Status.Conditions);
                }

                await client.PatchStatusAsync(device, patch, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
