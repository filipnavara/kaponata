// <copyright file="WebDriverResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// Represents the base payload of a WebDriver error or success response. In both cases, the
    /// root object contains a <see cref="Value"/> field.
    /// </summary>
    public class WebDriverResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverResponse"/> class.
        /// </summary>
        public WebDriverResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverResponse"/> class, and initializes
        /// the <see cref="Value"/> property.
        /// </summary>
        /// <param name="value">
        /// The initial value of the <see cref="Value"/> property.
        /// </param>
        public WebDriverResponse(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the embedded data.
        /// </summary>
        public object? Value { get; set; }
    }
}
