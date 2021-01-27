// <copyright file="WebDriverErrorData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// The WebDriver error data used when an error occurs.
    /// </summary>
    public class WebDriverErrorData : WebDriverData
    {
        /// <summary>
        /// Gets or sets the JSON error code which describes the error.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets an implementation-defined string with a human readable description of the kind of
        /// error that occurred.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets an implementation-defined string with a stack trace report of the active stack frames at
        /// the time when the error occurred.
        /// </summary>
        public string StackTrace { get; set; }
    }
}
