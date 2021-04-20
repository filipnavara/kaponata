// <copyright file="History.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// Describes the history of a layer.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/config.go#L63-L78"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/config.md"/>
    public class History
    {
        /// <summary>
        /// Gets or sets the date and time at which the layer was created.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the command which created the layer.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the author of the build point.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets a custom message set when creating the layer.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets a value used to mark if the history item created a filesystem diff.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("empty_layer")]
        public bool? EmptyLayer { get; set; }
    }
}
