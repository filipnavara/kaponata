// <copyright file="RepositoryCatalog.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// Describes the repository catalog.
    /// </summary>
    public class RepositoryCatalog
    {
        /// <summary>
        /// Gets or sets all repositories.
        /// </summary>
        [JsonPropertyName("repositories")]
        public List<string>? Repositories { get; set; }
    }
}
