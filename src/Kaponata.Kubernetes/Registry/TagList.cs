// <copyright file="TagList.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// A list of tags, as returned by the remote registry.
    /// </summary>
    public class TagList
    {
        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the list of tags in the repository.
        /// </summary>
        public List<string>? Tags { get; set; }
    }
}
