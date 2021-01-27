// <copyright file="WebDriverErrorResultTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using System;
using System.Net;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverErrorResult"/> class.
    /// </summary>
    public class WebDriverErrorResultTests
    {
        /// <summary>
        /// <see cref="WebDriverErrorResult"/> constructors validates the arguments
        /// passed to them.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("errorCode", () => new WebDriverErrorResult(null));
            Assert.Throws<ArgumentNullException>("errorCode", () => new WebDriverErrorResult(null, "message"));
            Assert.Throws<ArgumentNullException>("message", () => new WebDriverErrorResult(WebDriverErrorCode.InvalidArgument, null));
        }

        /// <summary>
        /// <see cref="WebDriverErrorResult"/> constructors initialize the error data.
        /// </summary>
        [Fact]
        public void Constructor_CopiesErrorCode()
        {
            var result = new WebDriverErrorResult(WebDriverErrorCode.InvalidArgument);

            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);

            var data = Assert.IsType<WebDriverErrorData>(result.Value);
            Assert.Equal("The arguments passed to a command are either invalid or malformed.", data.Message);
            Assert.Equal("invalid argument", data.Error);
            Assert.Null(data.Data);
            Assert.Null(data.StackTrace);
        }
    }
}
