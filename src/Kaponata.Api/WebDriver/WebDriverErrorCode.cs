// <copyright file="WebDriverErrorCode.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Net;

namespace Kaponata.Api.WebDriver
{
    /// <summary>
    /// Represents a well-known WebDriver error code.
    /// </summary>
    /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-error-response-data"/>
    public class WebDriverErrorCode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverErrorCode"/> class.
        /// </summary>
        /// <param name="jsonErrorCode">
        /// The JSON error code for this <see cref="WebDriverErrorCode"/>.
        /// </param>
        /// <param name="statusCode">
        /// The <see cref="HttpStatusCode"/> for this <see cref="WebDriverErrorCode"/>.
        /// </param>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        public WebDriverErrorCode(string jsonErrorCode, HttpStatusCode statusCode, string message)
        {
            this.JsonErrorCode = jsonErrorCode ?? throw new ArgumentNullException(nameof(jsonErrorCode));
            this.HttpStatusCode = statusCode;
            this.Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Gets the Invalid Argument error.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-invalid-argument"/>
        public static WebDriverErrorCode InvalidArgument { get; } =
            new WebDriverErrorCode(
                "invalid argument",
                HttpStatusCode.BadRequest,
                "The arguments passed to a command are either invalid or malformed.");

        /// <summary>
        /// Gets an error which occurs if the given session id is not in the list of active sessions,
        /// meaning the session either does not exist or that it’s not active.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-invalid-session-id"/>
        public static WebDriverErrorCode InvalidSessionId { get; }
            = new WebDriverErrorCode(
                "invalid session id",
                HttpStatusCode.NotFound,
                "The given session id is not in the list of active sessions.");

        /// <summary>
        /// Gets an error which occurs when a new session could not be created.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-session-not-created"/>
        public static WebDriverErrorCode SessionNotCreated { get; }
            = new WebDriverErrorCode(
                "session not created",
                HttpStatusCode.InternalServerError,
                "A new session could not be created.");

        /// <summary>
        /// Gets the error which occurs when an operation did not complete before its timeout expired.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-timeout"/>
        public static WebDriverErrorCode Timeout { get; }
            = new WebDriverErrorCode(
                "timeout",
                HttpStatusCode.InternalServerError,
                "An operation did not complete before its timeout expired.");

        /// <summary>
        /// Gets the error which occurs when a command could not be executed because the remote end is not aware of it.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-unknown-command"/>
        public static WebDriverErrorCode UnknownCommand { get; }
            = new WebDriverErrorCode(
                "unknown command",
                HttpStatusCode.NotFound,
                "A command could not be executed because the remote end is not aware of it.");

        /// <summary>
        /// Gets the error which occurs when an unknown error occurred in the while processing the command.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-unknown-error"/>
        public static WebDriverErrorCode UnknownError { get; }
            = new WebDriverErrorCode(
                "unknown error",
                HttpStatusCode.InternalServerError,
                "An unknown error occurred in the remote end while processing the command.");

        /// <summary>
        /// Gets an error which occurs when the requested command matched a known URL but did not match an method for that URL.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-unknown-method"/>
        public static WebDriverErrorCode UnknownMethod { get; }
            = new WebDriverErrorCode(
                "unknown method",
                HttpStatusCode.MethodNotAllowed,
                "The requested command matched a known URL but did not match an method for that URL.");

        /// <summary>
        /// Gets an error which occurs when a command that should have executed properly cannot be supported for some reason.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-unsupported-operation"/>.
        public static WebDriverErrorCode UnsupportedOperation { get; }
            = new WebDriverErrorCode(
                "unsupported operation",
                HttpStatusCode.InternalServerError,
                "The command that should have executed properly cannot be supported.");

        /// <summary>
        /// Gets the JSON error code for this <see cref="WebDriverErrorCode"/>.
        /// </summary>
        /// <seealso href="https://www.w3.org/TR/webdriver/#dfn-error-code"/>
        public string JsonErrorCode { get; }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> for this <see cref="WebDriverErrorCode"/>.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// Gets a non-normative description of the error.
        /// </summary>
        public string Message { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            var other = obj as WebDriverErrorCode;

            if (other == null)
            {
                return false;
            }

            return other.JsonErrorCode == this.JsonErrorCode
                && other.HttpStatusCode == this.HttpStatusCode;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.JsonErrorCode, this.HttpStatusCode);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.JsonErrorCode;
        }
    }
}
