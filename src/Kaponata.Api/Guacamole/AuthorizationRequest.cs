// <copyright file="AuthorizationRequest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Kaponata.Api.Guacamole
{
    /// <summary>
    /// Represents a request to authorize a subject.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// Gets or sets the user name of the subject.
        /// </summary>
        [JsonPropertyName("username")]
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for the user, if any.
        /// </summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the network address at which the subject is located, if available.
        /// </summary>
        [JsonPropertyName("remoteAddress")]
        public string? RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the network host at which the subject is located, if available.
        /// </summary>
        [JsonPropertyName("remoteHostname")]
        public string? RemoteHostName { get; set; }

        /// <summary>
        /// Gets or sets a list of HTTP headers associated with the request, and their values.
        /// </summary>
        /// <remarks>
        /// Because a given HTTP header may have multiple values, the object associated with a header name is an array
        /// of strings, even if there is just one value for a given header name.
        /// </remarks>
        [JsonPropertyName("headers")]
        public Dictionary<string, Collection<string>>? Headers { get; set; }
    }
}
