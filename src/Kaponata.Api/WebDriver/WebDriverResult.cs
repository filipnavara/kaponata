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
        /// The WebDriver-formatted result data.
        /// </param>
        public WebDriverResult(WebDriverResponse data)
            : base(data)
        {
            this.SerializerSettings = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            this.ContentType = "application/json; charset=utf-8";

            switch (data.Value)
            {
                case WebDriverError errorData when data.Value is WebDriverError:
                    this.StatusCode = (int)errorData.ErrorCode.HttpStatusCode;
                    break;

                default:
                    this.StatusCode = 200;
                    break;
            }
        }
    }
}
