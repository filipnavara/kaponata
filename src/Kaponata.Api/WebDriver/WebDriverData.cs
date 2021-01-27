// <copyright file="WebDriverData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// Represents the base payload of a WebDriver error or success response. In both cases, the
    /// root object contains a <see cref="Data"/> field.
    /// </summary>
    public class WebDriverData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverData"/> class.
        /// </summary>
        public WebDriverData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverData"/> class, and initializes
        /// the <see cref="Data"/> property.
        /// </summary>
        /// <param name="data">
        /// The initial value of the <see cref="Data"/> property.
        /// </param>
        public WebDriverData(object data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the embedded data.
        /// </summary>
        public object Data { get; set; }
    }
}
