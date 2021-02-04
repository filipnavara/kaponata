﻿// <copyright file="WebDriverSessionStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace Kaponata.Operator.Models
{
    /// <summary>
    /// Describes the status of a <see cref="WebDriverSession"/> object.
    /// </summary>
    public class WebDriverSessionStatus
    {
        /// <summary>
        /// Gets or sets the session ID used to uniquely identify the WebDriver session.
        /// </summary>
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the session capabilities, as determined by the server.
        /// </summary>
        [JsonProperty("capabilities")]
        public string Capabilities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is ready on the back-end pod.
        /// </summary>
        [JsonProperty("sessionReady")]
        public bool SessionReady { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ingress rules are ready.
        /// </summary>
        [JsonProperty("ingressReady")]
        public bool IngressReady { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether service is ready.
        /// </summary>
        [JsonProperty("serviceReady")]
        public bool ServiceReady { get; set; }
    }
}
