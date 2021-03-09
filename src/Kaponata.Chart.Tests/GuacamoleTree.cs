// <copyright file="GuacamoleTree.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// A Guacamole connection tree.
    /// </summary>
    public class GuacamoleTree
    {
        /// <summary>
        /// Gets or sets the user-friendly name of the connection tree.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the connection tree.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the type of the connection tree.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the number of active connections.
        /// </summary>
        [JsonProperty("activeConnections")]
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Gets or sets additional attributes.
        /// </summary>
        [JsonProperty("attributes")]
        public object Attributes { get; set; }
    }
}
