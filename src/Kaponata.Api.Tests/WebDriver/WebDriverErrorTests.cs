// <copyright file="WebDriverErrorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using System;
using System.Net;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverError"/> class.
    /// </summary>
    public class WebDriverErrorTests
    {
        /// <summary>
        /// <see cref="WebDriverError"/> constructors validates the arguments
        /// passed to them.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("errorCode", () => new WebDriverError((WebDriverErrorCode)null));
            Assert.Throws<ArgumentNullException>("errorCode", () => new WebDriverError(null, "message"));
            Assert.Throws<ArgumentNullException>("message", () => new WebDriverError(WebDriverErrorCode.InvalidArgument, null));
        }

        /// <summary>
        /// <see cref="WebDriverError"/> constructors initialize the error data.
        /// </summary>
        [Fact]
        public void Constructor_CopiesErrorCode()
        {
            var result = new WebDriverError(WebDriverErrorCode.InvalidArgument);

            Assert.Same(WebDriverErrorCode.InvalidArgument, result.ErrorCode);

            Assert.Equal("The arguments passed to a command are either invalid or malformed.", result.Message);
            Assert.Equal("invalid argument", result.Error);
            Assert.Null(result.Data);
            Assert.Null(result.StackTrace);
        }
    }
}
