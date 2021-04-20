// <copyright file="RootFS.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// RootFS describes the layer content addresses used by the image.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/config.go#L53-L60"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/config.md"/>
    public class RootFS
    {
        /// <summary>
        /// Gets or sets the type of <see cref="RootFS"/>.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "layers";

        /// <summary>
        /// Gets or sets an array of layer content hashes (DiffIDs), in order from bottom-most to top-most.
        /// </summary>
        [JsonPropertyName("diff_ids")]
        public string[] DiffIDs { get; set; } = Array.Empty<string>();
    }
}
