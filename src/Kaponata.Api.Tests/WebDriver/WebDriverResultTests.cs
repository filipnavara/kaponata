// <copyright file="WebDriverResultTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverResult"/> class.
    /// </summary>
    public class WebDriverResultTests
    {
        /// <summary>
        /// The <see cref="WebDriverResult.WebDriverResult(WebDriverResponse)"/> constructor
        /// copies the <see cref="WebDriverResponse"/> value passed to it.
        /// </summary>
        [Fact]
        public void Constructor_CopiesData()
        {
            var data = new WebDriverResponse();

            WebDriverResult result = new WebDriverResult(data);
            Assert.Same(data, result.Value);
            Assert.Equal("application/json; charset=utf-8", result.ContentType);
            Assert.Equal(200, result.StatusCode);
        }

        /// <summary>
        /// The <see cref="WebDriverResult.WebDriverResult(WebDriverResponse)"/> constructor
        /// properly initializes the status code when processing errors.
        /// </summary>
        [Fact]
        public void Constructor_Error()
        {
            var data = new WebDriverResponse(
                new WebDriverError(WebDriverErrorCode.InvalidSessionId));

            WebDriverResult result = new WebDriverResult(data);
            Assert.Same(data, result.Value);
            Assert.Equal("application/json; charset=utf-8", result.ContentType);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
