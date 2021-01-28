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
        /// The <see cref="WebDriverResult.WebDriverResult(object)"/> constructor
        /// embeds the data in a <see cref="WebDriverData"/> class.
        /// </summary>
        [Fact]
        public void Constructor_EmbedsData()
        {
            string value = "test";

            WebDriverResult result = new WebDriverResult(value);
            var data = Assert.IsType<WebDriverData>(result.Value);
            Assert.Equal(value, data.Data);
        }

        /// <summary>
        /// The <see cref="WebDriverResult.WebDriverResult(object)"/> constructor
        /// copies the <see cref="WebDriverData"/> value passed to it.
        /// </summary>
        [Fact]
        public void Constructor_CopiesData()
        {
            var data = new WebDriverData();

            WebDriverResult result = new WebDriverResult(data);
            Assert.Same(data, result.Value);
        }

        /// <summary>
        /// The <see cref="WebDriverResult"/> constructor initializes the <see cref="JsonResult.Value"/>
        /// to an empty <see cref="WebDriverData"/> object.
        /// </summary>
        [Fact]
        public void Constructor_InitializesEmptyData()
        {
            var result = new WebDriverResult((WebDriverData)null);
            Assert.IsType<WebDriverData>(result.Value);

            result = new WebDriverResult((object)null);
            Assert.IsType<WebDriverData>(result.Value);
        }
    }
}
