// <copyright file="Platform.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// <see cref="Platform"/> describes the platform which the image in the manifest runs on.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/descriptor.go#L45-L63"/>
    public class Platform
    {
        /// <summary>
        /// Gets or sets the the CPU architecture, for example <see cref="Annotations.Architecture.Amd64"/>.
        /// </summary>
        [JsonPropertyName("architecture")]
        public string Architecture { get; set; } = Annotations.Architecture.Amd64;

        /// <summary>
        /// Gets or sets the operating system, for example <see cref="Annotations.OperatingSystem.Linux"/>.
        /// </summary>
        [JsonPropertyName("os")]
        public string OS { get; set; } = Annotations.OperatingSystem.Linux;

        /// <summary>
        /// Gets or sets an optional value specifying the operating system
        /// version, for example on Windows <c>10.0.14393.1066</c>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("os.version")]
        public string? OSVersion { get; set; }

        /// <summary>
        /// Gets or sets an optional field specifying an array of strings,
        /// each listing a required OS feature (for example on Windows <c>win32k</c>).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("os.features")]
        public string[]? OSFeatures { get; set; }

        /// <summary>
        /// Gets or sets an optional field specifying a variant of the CPU, for
        /// example `v7` to specify ARMv7 when architecture is <see cref="Annotations.Architecture.Arm64"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("variant")]
        public string? Variant { get; set; }
    }
}
