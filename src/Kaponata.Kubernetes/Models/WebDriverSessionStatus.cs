// <copyright file="WebDriverSessionStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;

#nullable disable

namespace Kaponata.Kubernetes.Models
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
        /// Gets or sets the TCP port number at which the Appium server is listening.
        /// </summary>
        [JsonProperty("sessionPort")]
        public int SessionPort { get; set; }

        /// <summary>
        /// Gets or sets a string indicating the error code. The session failed to start if this
        /// value is set.
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets an implementation-defined string with a human readable description of the kind of error that occurred.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets an implementation-defined string with a stack trace report of the active stack frames at the time when the error occurred.
        /// </summary>
        [JsonProperty("stacktrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets additional error data helpful in diagnosing the error.
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; }

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
