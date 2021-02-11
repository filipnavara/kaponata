// <copyright file="NewSessionRequest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// The request data for the new session request.
    /// </summary>
    public class NewSessionRequest
    {
        /// <summary>
        /// Gets or sets the capabilities requested by the client.
        /// </summary>
        public CapabilitiesRequest? Capabilities { get; set; }
    }
}
