// <copyright file="WebDriverController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// The <see cref="WebDriverController"/> handles WebDriver requests.
    /// </summary>
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None)]
    public class WebDriverController : Controller
    {
        private readonly ILogger<WebDriverController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverController"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public WebDriverController(ILogger<WebDriverController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchrounsly creates a new session.
        /// </summary>
        /// <param name="request">
        /// The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <seealso href="https://www.w3.org/TR/webdriver/#new-session"/>
        [HttpPost("/wd/hub/session")]
        public Task<WebDriverResult> NewSessionAsync(object request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new WebDriverResult(
                    new WebDriverResponse(
                        new WebDriverError(
                        WebDriverErrorCode.SessionNotCreated,
                        "This version of Kaponata does not support creating any sessions."))));
        }

        /// <summary>
        /// Asynchronously deletes a session.
        /// </summary>
        /// <param name="sessionId">
        /// The ID of the session to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <seealso href="https://www.w3.org/TR/webdriver/#delete-session"/>
        [HttpDelete("/wd/hub/session/{sessionId}")]
        public Task<WebDriverResult> DeleteAsync(string sessionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new WebDriverResult(
                    new WebDriverResponse(
                        new WebDriverError(
                    WebDriverErrorCode.InvalidSessionId,
                    $"A session with session id '{sessionId}' could not be found"))));
        }

        /// <summary>
        /// Asynchronously returns the status of this node.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        [HttpGet("/wd/hub/status")]
        public Task<WebDriverResult> StatusAsync(CancellationToken cancellationToken)
    {
            return Task.FromResult(
                new WebDriverResult(
                    new WebDriverResponse(
                        new WebDriverStatus()
                            {
                                Ready = false,
                                Message = "This version of Kaponata does not support creating any sessions.",
                            })));
        }
    }
}
