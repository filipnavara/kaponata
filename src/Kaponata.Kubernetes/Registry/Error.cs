// <copyright file="Error.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// Represents an error returned by the registry.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Gets or sets a code which uniquely identifies the error.
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets a message which describes the error.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets additional error details.
        /// </summary>
        [JsonPropertyName("detail")]
        public object? Detail { get; set; }
    }
}
