// <copyright file="ImageConfig.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// <see cref="ImageConfig"/> defines the execution parameters which should be used as a base when running a container using an image.
    /// </summary>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/859973e32ccae7b7fc76b40b762c9fff6e912f9e/config.md"/>
    /// <seealso href="https://github.com/opencontainers/image-spec/blob/master/specs-go/v1/config.go#L24-L51"/>
    public class ImageConfig
    {
        /// <summary>
        /// Gets or sets the username or UID which the process in the container should run as.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("User")]
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets a set of ports to expose from a container running this image. The keys can be in the format of:
        /// <c>port/tcp</c>, <c>port/udp</c>, <c>port</c> with the default protocol being tcp if not specified.
        /// These values act as defaults and are merged with any specified when creating a container.
        /// </summary>
        /// <remarks>
        /// This JSON structure value is unusual because it is a direct JSON serialization of a dictionary and is
        /// represented in JSON as an object mapping its keys to an empty object.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("ExposedPorts")]
        public Dictionary<string, object>? ExposedPorts { get; set; }

        /// <summary>
        /// Gets or sets a list of environment variables to be used in a container.
        /// </summary>
        /// <remarks>
        /// Entries are in the format of <c>VARNAME=VARVALUE</c>. These values act as defaults and are merged
        /// with any specified when creating a container.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Env")]
        public string[]? Env { get; set; }

        /// <summary>
        /// Gets or sets a list of arguments to use as the command to execute when the container starts.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Entrypoint")]
        public string[]? Entrypoint { get; set; }

        /// <summary>
        /// Gets or sets the default arguments to the entrypoint of the container.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Cmd")]
        public string[]? Cmd { get; set; }

        /// <summary>
        /// Gets or sets a set of directories describing where the process is likely write data specific to a container instance.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Volumes")]
        public Dictionary<string, object>? Volumes { get; set; }

        /// <summary>
        /// Gets or sets the current working directory of the entrypoint process in the container.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("WorkingDir")]
        public string? WorkingDir { get; set; }

        /// <summary>
        /// Gets or sets arbitrary metadata for the container.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Labels")]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>
        /// Gets or sets the system call signal that will be sent to the container to exit.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("StopSignal")]
        public string? StopSignal { get; set; }
    }
}