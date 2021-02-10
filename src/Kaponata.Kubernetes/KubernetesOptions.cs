// <copyright file="KubernetesOptions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Contains options used by the <see cref="KubernetesClient"/>.
    /// </summary>
    public class KubernetesOptions
    {
        /// <summary>
        /// Gets the default options.
        /// </summary>
        public static readonly IOptions<KubernetesOptions> Default = new OptionsWrapper<KubernetesOptions>(new KubernetesOptions());

        /// <summary>
        /// Gets or sets the name of the namespace in which the <see cref="KubernetesClient"/> operates.
        /// The default value is <c>default</c>.
        /// </summary>
        public string Namespace { get; set; } = "default";
    }
}
