// <copyright file="WebDriverSessionSpec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace Kaponata.Operator.Models
{
    /// <summary>
    /// Defines the specifications (invariant data) related to a <see cref="WebDriverSession"/> object.
    /// </summary>
    public class WebDriverSessionSpec
    {
        /// <summary>
        /// Gets or sets the desired capabilities, as requested by the client.
        /// </summary>
        [JsonProperty(PropertyName = "desiredCapabilities")]
        public string DesiredCapabilities { get; set; }
    }
}
