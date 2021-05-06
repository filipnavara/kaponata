// <copyright file="ImageRegistryException.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// Represents errors that occur when interacting with an image registry.
    /// </summary>
    public class ImageRegistryException : Exception
    {
        private const string DefaultMessage = "An unexpected error occurred when sending a request to the remote server.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryException"/> class.
        /// </summary>
        /// <param name="statusCode">
        /// The HTTP status code returned by the remote server.
        /// </param>
        public ImageRegistryException(HttpStatusCode statusCode)
            : this(statusCode, DefaultMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryException"/> class,
        /// using an error message.
        /// </summary>
        /// <param name="statusCode">
        /// The HTTP status code returned by the remote server.
        /// </param>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        public ImageRegistryException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets, when available, the status code returned by the remote server.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets, when available, a list of <see cref="Error" /> objects which describe the error.
        /// </summary>
        public List<Error> Errors { get; private set; } = new List<Error>();

        /// <summary>
        /// Creates a new <see cref="ImageRegistryException"/> object based on a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="response">
        /// The response sent by the server.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the <see cref="ImageRegistryException"/>
        /// when completed.
        /// </returns>
        public static async Task<ImageRegistryException> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content == null || response.Content.Headers.ContentLength == 0)
            {
                return new ImageRegistryException(response.StatusCode);
            }
            else if (response.Content.Headers?.ContentType?.MediaType != "application/json")
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return new ImageRegistryException(response.StatusCode, message);
            }
            else
            {
                var errors = await response.Content.ReadFromJsonAsync<ErrorList>(null, cancellationToken).ConfigureAwait(false);
                var message = errors?.Errors?[0].Message ?? DefaultMessage;

                var exception = new ImageRegistryException(response.StatusCode, message);

                if (errors?.Errors != null)
                {
                    exception.Errors.AddRange(errors.Errors);
                }

                return exception;
            }
        }
    }
}
