// <copyright file="WebDriverStatus.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// Describes the readiness state of a WebDriver node.
    /// </summary>
    /// <seealso href="https://www.w3.org/TR/webdriver/#nodes"/>
    public class WebDriverStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether this node is ready.
        /// </summary>
        public bool Ready { get; set; }

        /// <summary>
        /// Gets or sets a message which describes the state.
        /// </summary>
        public string Message { get; set; }
    }
}
