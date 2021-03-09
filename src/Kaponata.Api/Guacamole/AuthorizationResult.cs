// <copyright file="AuthorizationResult.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kaponata.Api.Guacamole
{
    /// <summary>
    /// Represents the result of an authorization request.
    /// </summary>
    public class AuthorizationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the authorization request was successful.
        /// </summary>
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }

        /// <summary>
        /// Gets or sets the list of authorized connection resources for the authorized user.
        /// </summary>
        /// <remarks>
        /// This value is included only for an authorized subject.
        /// </remarks>
        [JsonPropertyName("configurations")]
        public Dictionary<string, Configuration>? Configurations { get; set; }
    }
}
