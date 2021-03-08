// <copyright file="GuacamoleController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.Guacamole;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// Implements an REST API which allows Guacamole to authenticate users using the guacamole-auth-rest extension.
    /// </summary>
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None)]
    public class GuacamoleController
    {
        private readonly ILogger<GuacamoleController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuacamoleController"/> class.
        /// </summary>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        public GuacamoleController(ILogger<GuacamoleController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="body">
        /// The authorization request.
        /// </param>
        /// <returns>
        /// A <see cref="AuthorizationResult"/> which contains the result of the authentication operation.
        /// </returns>
        [HttpPost("/api/guacamole/authorization")]
        public ActionResult Authorize([FromBody] AuthorizationRequest body)
        {
            var result = new AuthorizationResult
            {
                Authorized = true,
                Configurations = new Dictionary<string, Configuration>(),
            };

            return new OkObjectResult(result);
        }
    }
}