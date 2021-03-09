// <copyright file="Configuration.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kaponata.Api.Guacamole
{
    /// <summary>
    /// Represents an authorized connection resource.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the Guacamole protocol name; e.g. RDP, VNC or SSH.
        /// </summary>
        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }

        /// <summary>
        /// Gets or sets a dictionary which contains parameter names and corresponding value types for this configuration.
        /// </summary>
        /// <remarks>
        /// These values are protocol-specific, and are typically defined and described in the Guacamole documentation for a supported protocol.
        /// The value data types may be either strings, numbers, or booleans. This structure corresponds to the <c>GuacamoleConfiguration</c>
        /// defined in guacamole-common.
        /// </remarks>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
