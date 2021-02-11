// <copyright file="NewSessionResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// The response for a new session request.
    /// </summary>
    public class NewSessionResponse
    {
        /// <summary>
        /// Gets or sets the ID of the newly created session.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Gets or sets the session capabilities.
        /// </summary>
        public Dictionary<string, object>? Capabilities { get; set; }
    }
}
