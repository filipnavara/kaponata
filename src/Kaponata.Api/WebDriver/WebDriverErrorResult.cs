// <copyright file="WebDriverErrorResult.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// An action results which returns a WebDriver errror to the client.
    /// </summary>
    public class WebDriverErrorResult : WebDriverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverErrorResult"/>
        /// class.
        /// </summary>
        /// <param name="errorCode">
        /// The error code which describes the error.
        /// </param>
        public WebDriverErrorResult(WebDriverErrorCode errorCode)
            : this(errorCode, errorCode?.Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverErrorResult"/>
        /// class.
        /// </summary>
        /// <param name="errorCode">
        /// The error code which describes the error.
        /// </param>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        public WebDriverErrorResult(WebDriverErrorCode errorCode, string message)
            : base(new WebDriverErrorData())
        {
            if (errorCode == null)
            {
                throw new ArgumentNullException(nameof(errorCode));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var errorData = (WebDriverErrorData)this.Value;
            errorData.Message = message;
            errorData.Error = errorCode.JsonErrorCode;

            this.StatusCode = (int)errorCode.HttpStatusCode;
        }
    }
}
