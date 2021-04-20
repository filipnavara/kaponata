// <copyright file="Image.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Text.Json.Serialization;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// Represents the JSON structure which describes some basic information about a container image.
    /// This provides the <c>application/vnd.oci.image.config.v1+json</c> mediatype when serialized to JSON.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/config.md"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/config.go#L80-L103"/>
    public class Image
    {
        /// <summary>
        /// Gets or sets the date and time at which the image was created.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the name and/or email address of the person or entity which created and is responsible for maintaining the image.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the CPU architecture which the binaries in this image are built to run on.
        /// </summary>
        /// <remarks>
        /// Should be one of the <see cref="Annotations.Architecture"/> values.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("architecture")]
        public string Architecture { get; set; } = Annotations.Architecture.Amd64;

        /// <summary>
        /// Gets or sets the name of the operating system which the image is built to run on.
        /// </summary>
        /// <remarks>
        /// Should be one of the <see cref="Annotations.OperatingSystem"/> values.</remarks>
        [JsonPropertyName("os")]
        public string OS { get; set; } = Annotations.OperatingSystem.Linux;

        /// <summary>
        /// Gets or sets the execution parameters which should be used as a base when running a container using the image.
        /// This property can be <see langword="null"/>, in which case any execution parameters should be specified at
        /// creation of the container.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("config")]
        public ImageConfig? Config { get; set; }

        /// <summary>
        /// Gets or sets the layer content addresses used by the image.
        /// </summary>
        [JsonPropertyName("rootfs")]
        public RootFS RootFS { get; set; } = new RootFS();

        /// <summary>
        /// Gets or sets a description of the history of each layer.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("history")]
        public History[]? History { get; set; }
    }
}
