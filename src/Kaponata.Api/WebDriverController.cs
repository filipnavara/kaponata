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
        private readonly KubernetesWebDriver webDriver;
        private readonly ILogger<WebDriverController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverController"/> class.
        /// </summary>
        /// <param name="webDriver">
        /// The <see cref="KubernetesWebDriver"/> which contains the back-end WebDriver implementation.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public WebDriverController(KubernetesWebDriver webDriver, ILogger<WebDriverController> logger)
        {
            this.webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
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
        public async Task<WebDriverResult> NewSessionAsync(NewSessionRequest request, CancellationToken cancellationToken)
        {
            var response = await this.webDriver.CreateSessionAsync(request, cancellationToken).ConfigureAwait(false);
            return new WebDriverResult(response);
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
        public async Task<WebDriverResult> DeleteAsync(string sessionId, CancellationToken cancellationToken)
        {
            var response = await this.webDriver.DeleteSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
            return new WebDriverResult(response);
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
