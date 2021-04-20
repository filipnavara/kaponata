// <copyright file="Descriptor.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// <see cref="Descriptor"/> describes the disposition of targeted content.
    /// This structure provides the <c>application/vnd.oci.descriptor.v1+json</c> mediatype
    /// when marshalled to JSON.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/descriptor.go"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/descriptor.md#properties"/>
    public class Descriptor
    {
        /// <summary>
        /// Gets or sets the media type of the object this schema refers to.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the digest of the targeted content.
        /// </summary>
        [JsonPropertyName("digest")]
        public string Digest { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size in bytes of the blob.digest.
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets a list of URLs from which this object MAY be downloaded.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("urls")]
        public string[]? URLs { get; set; }

        /// <summary>
        /// Gets or sets a list of arbitrary metadata relating to the targeted content.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("annotations")]
        public Dictionary<string, string>? Annotations { get; set; }

        /// <summary>
        /// Gets or sets the platform which the image in the manifest runs on.
        /// This should only be used when referring to a manifest.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("platform")]
        public Platform? Platform { get; set; }
    }
}
