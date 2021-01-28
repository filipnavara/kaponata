// <copyright file="WebDriverErrorCodeTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using System;
using System.Net;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverErrorCode"/> class.
    /// </summary>
    public class WebDriverErrorCodeTests
    {
        /// <summary>
        /// The <see cref="WebDriverErrorCode"/> constructor validates the arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("jsonErrorCode", () => new WebDriverErrorCode(null, HttpStatusCode.OK, "OK"));
            Assert.Throws<ArgumentNullException>("message", () => new WebDriverErrorCode("ok", HttpStatusCode.OK, null));
        }

        /// <summary>
        /// The <see cref="WebDriverErrorCode"/> constructor initializes the properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsProperties()
        {
            var error = new WebDriverErrorCode("code", HttpStatusCode.BadRequest, "test");
            Assert.Equal("code", error.JsonErrorCode);
            Assert.Equal(HttpStatusCode.BadRequest, error.HttpStatusCode);
            Assert.Equal("test", error.Message);
        }

        /// <summary>
        /// <see cref="WebDriverErrorCode.Equals(object)"/> works correctly.
        /// </summary>
        [Fact]
        public void Equals_Works()
        {
            var invalidArgument = new WebDriverErrorCode(
                "invalid argument",
                HttpStatusCode.BadRequest,
                "The arguments passed to a command are either invalid or malformed.");

            Assert.True(WebDriverErrorCode.InvalidArgument.Equals(invalidArgument));
            Assert.True(invalidArgument.Equals(WebDriverErrorCode.InvalidArgument));

            Assert.False(invalidArgument.Equals(WebDriverErrorCode.InvalidSessionId));
            Assert.False(invalidArgument.Equals(null));
            Assert.False(invalidArgument.Equals("invalid session"));
        }

        /// <summary>
        /// <see cref="WebDriverErrorCode.GetHashCode"/> works correctly.
        /// </summary>
        [Fact]
        public void GetHashCode_Works()
        {
            var invalidArgument = new WebDriverErrorCode(
                "invalid argument",
                HttpStatusCode.BadRequest,
                "The arguments passed to a command are either invalid or malformed.");

            Assert.Equal(WebDriverErrorCode.InvalidArgument.GetHashCode(), invalidArgument.GetHashCode());
            Assert.Equal(invalidArgument.GetHashCode(), WebDriverErrorCode.InvalidArgument.GetHashCode());

            Assert.NotEqual(invalidArgument.GetHashCode(), WebDriverErrorCode.InvalidSessionId.GetHashCode());
        }

        /// <summary>
        /// <see cref="WebDriverErrorCode.ToString()"/> method returns the JSON error code.
        /// </summary>
        [Fact]
        public void ToString_ReturnsErrorCode()
        {
            var error = new WebDriverErrorCode("code", HttpStatusCode.BadRequest, "test");
            Assert.Equal("code", error.ToString());
        }

        /// <summary>
        /// The well-known errors are correct.
        /// </summary>
        [Fact]
        public void KnownErrors_AreCorrect()
        {
            Assert.Equal("invalid argument", WebDriverErrorCode.InvalidArgument.JsonErrorCode);
            Assert.Equal(HttpStatusCode.BadRequest, WebDriverErrorCode.InvalidArgument.HttpStatusCode);

            Assert.Equal("invalid session id", WebDriverErrorCode.InvalidSessionId.JsonErrorCode);
            Assert.Equal(HttpStatusCode.NotFound, WebDriverErrorCode.InvalidSessionId.HttpStatusCode);

            Assert.Equal("session not created", WebDriverErrorCode.SessionNotCreated.JsonErrorCode);
            Assert.Equal(HttpStatusCode.InternalServerError, WebDriverErrorCode.SessionNotCreated.HttpStatusCode);

            Assert.Equal("timeout", WebDriverErrorCode.Timeout.JsonErrorCode);
            Assert.Equal(HttpStatusCode.InternalServerError, WebDriverErrorCode.Timeout.HttpStatusCode);

            Assert.Equal("unknown command", WebDriverErrorCode.UnknownCommand.JsonErrorCode);
            Assert.Equal(HttpStatusCode.NotFound, WebDriverErrorCode.UnknownCommand.HttpStatusCode);

            Assert.Equal("unknown error", WebDriverErrorCode.UnknownError.JsonErrorCode);
            Assert.Equal(HttpStatusCode.InternalServerError, WebDriverErrorCode.UnknownError.HttpStatusCode);

            Assert.Equal("unknown method", WebDriverErrorCode.UnknownMethod.JsonErrorCode);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, WebDriverErrorCode.UnknownMethod.HttpStatusCode);

            Assert.Equal("unsupported operation", WebDriverErrorCode.UnsupportedOperation.JsonErrorCode);
            Assert.Equal(HttpStatusCode.InternalServerError, WebDriverErrorCode.UnsupportedOperation.HttpStatusCode);
        }
    }
}
