// <copyright file="GuacamoleController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.Guacamole;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
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
        private readonly KubernetesClient kubernetesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuacamoleController"/> class.
        /// </summary>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        /// <param name="kubernetesClient">
        /// A <see cref="KubernetesClient"/> which provides connectivity to the Kubernetes client.
        /// </param>
        public GuacamoleController(ILogger<GuacamoleController> logger, KubernetesClient kubernetesClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
        }

        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="body">
        /// The authorization request.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="AuthorizationResult"/> which contains the result of the authentication operation.
        /// </returns>
        [HttpPost("/api/guacamole/authorization")]
        public async Task<ActionResult> AuthorizeAsync([FromBody] AuthorizationRequest body, CancellationToken cancellationToken)
        {
            var deviceClient = this.kubernetesClient.GetClient<MobileDevice>();
            var devices = await deviceClient.ListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var result = new AuthorizationResult
            {
                Authorized = true,
                Configurations = new Dictionary<string, Configuration>(),
            };

            foreach (var device in devices.Items)
            {
                if (device.Status?.VncHost != null)
                {
                    result.Configurations.Add(
                        device.Metadata.Name,
                        new Configuration()
                        {
                            Protocol = "vnc",
                            Parameters = new Dictionary<string, object>()
                            {
                                { "hostname", device.Status.VncHost },
                                { "port", 5900 },
                                { "password", string.Empty },
                            },
                        });
                }
            }

            return new OkObjectResult(result);
        }
    }
}