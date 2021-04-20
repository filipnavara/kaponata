// <copyright file="Manifest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// <see cref="Manifest"/> provides the <c>application/vnd.oci.image.manifest.v1+json</c> mediatype structure
    /// when converted to JSON.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/manifest.md"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/manifest.go"/>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the image manifest schema that this image follows.
        /// </summary>
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets a configuration object for a container, by digest.
        /// </summary>
        [JsonPropertyName("config")]
        public Descriptor Config { get; set; } = new Descriptor();

        /// <summary>
        /// Gets or sets an indexed list of layers referenced by the manifest.
        /// </summary>
        [JsonPropertyName("layers")]
        public Descriptor[] Layers { get; set; } = Array.Empty<Descriptor>();

        /// <summary>
        /// Gets or sets arbitrary metadata for the image manifest.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("annotations")]
        public Dictionary<string, string>? Annotations { get; set; }
    }
}
