// <copyright file="WebDriverError.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// The WebDriver error data used when an error occurs.
    /// </summary>
    /// <seealso href="https://github.com/jlipps/simple-wd-spec#error-handling"/>
    /// <seealso href="https://www.w3.org/TR/webdriver/#errors"/>
    public class WebDriverError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverError"/> class.
        /// </summary>
        /// <param name="errorCode">
        /// The error code which represents the error.
        /// </param>
        public WebDriverError(WebDriverErrorCode errorCode)
        {
            this.ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            this.Message = errorCode.Message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverError"/> class.
        /// </summary>
        /// <param name="errorCode">
        /// The error code which represents the error.
        /// </param>
        /// <param name="message">
        /// An implementation-specific error code for the error.
        /// </param>
        public WebDriverError(WebDriverErrorCode errorCode, string message)
        {
            this.ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            this.Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Gets the <see cref="WebDriverErrorCode"/> which represents the error.
        /// </summary>
        [JsonIgnore]
        public WebDriverErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets the JSON error code which describes the error.
        /// </summary>
        public string Error => this.ErrorCode.JsonErrorCode;

        /// <summary>
        /// Gets or sets an implementation-defined string with a human readable description of the kind of
        /// error that occurred.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets an implementation-defined string with a stack trace report of the active stack frames at
        /// the time when the error occurred.
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Gets or sets an object with additional error data helpful in diagnosing the error.
        /// </summary>
        public object? Data { get; set; }
    }
}
