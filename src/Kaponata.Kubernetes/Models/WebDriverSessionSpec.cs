﻿// <copyright file="WebDriverSessionSpec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

#nullable disable

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// Defines the specifications (invariant data) related to a <see cref="WebDriverSession"/> object.
    /// </summary>
    public class WebDriverSessionSpec
    {
        /// <summary>
        /// Gets or sets the capabilities requested by the client.
        /// </summary>
        [JsonProperty(PropertyName = "capabilities")]
        public string Capabilities { get; set; }
    }
}
