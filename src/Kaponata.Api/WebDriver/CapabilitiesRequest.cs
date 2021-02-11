// <copyright file="CapabilitiesRequest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// The capabilities requested by the client.
    /// </summary>
    public class CapabilitiesRequest
    {
        /// <summary>
        /// Gets or sets the capabilities which must always be matched.
        /// </summary>
        public Dictionary<string, object>? AlwaysMatch { get; set; }

        /// <summary>
        /// Gets or sets a list of capabilities of which the first item which an be matched
        /// should be used.
        /// </summary>
        public IEnumerable<Dictionary<string, object>>? FirstMatch { get; set; }
    }
}
