// <copyright file="WebDriverResult.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// Sends the result of a WebDriver to a client.
    /// </summary>
    public class WebDriverResult : JsonResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverResult"/> class.
        /// </summary>
        /// <param name="data">
        /// The result data which will be embedded in the <see cref="JsonResult.Value"/>
        /// property.
        /// </param>
        public WebDriverResult(object data)
            : this(new WebDriverData(data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverResult"/> class.
        /// </summary>
        /// <param name="data">
        /// The WebDriver-formatted result data.
        /// </param>
        public WebDriverResult(WebDriverData data)
            : base(data ?? new WebDriverData())
        {
            this.SerializerSettings = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            this.ContentType = "application/json; charset=utf-8";
            this.StatusCode = 200;
        }
    }
}
