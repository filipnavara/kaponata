// <copyright file="KubernetesAdbContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// The <see cref="KubernetesAdbContext"/> configures the <see cref="KubernetesAdbSocketLocator"/>.
    /// </summary>
    public class KubernetesAdbContext
    {
        /// <summary>
        /// Gets or sets the <see cref="V1Pod"/> on which the adb instance is running.
        /// </summary>
        public V1Pod Pod { get; set; }
    }
}
