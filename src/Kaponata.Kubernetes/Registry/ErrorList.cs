// <copyright file="ErrorList.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// A list of errors returned by the registry client, when an operation fails.
    /// </summary>
    public class ErrorList
    {
        /// <summary>
        /// Gets or sets the list of errors returned by the registry client.
        /// </summary>
        [JsonPropertyName("errors")]
        public List<Error>? Errors { get; set; }
    }
}
