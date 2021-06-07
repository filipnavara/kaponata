// <copyright file="LicenseController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Licensing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaponata.Api
{
    /// <summary>
    /// Provides routes for working with license files.
    /// </summary>
    public class LicenseController : Controller
    {
        private readonly LicenseStore licenseStore;
        private readonly ILogger<LicenseController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseController"/> class.
        /// </summary>
        /// <param name="licenseStore">
        /// A <see cref="LicenseStore"/> which is used to store the Kaponata license.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging messages.
        /// </param>
        public LicenseController(LicenseStore licenseStore, ILogger<LicenseController> logger)
        {
            this.licenseStore = licenseStore ?? throw new ArgumentNullException(nameof(licenseStore));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Updates the current license.
        /// </summary>
        /// <param name="license">
        /// The license to be used.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [HttpPost]
        [Route("api/license")]
        public async Task<ActionResult> UpdateLicenseAsync(IFormFile license, CancellationToken cancellationToken)
        {
            if (license == null)
            {
                return this.BadRequest("The license must be posed as a form file named 'license'");
            }

            using (Stream stream = license.OpenReadStream())
            {
                var licenseDocument = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
                await this.licenseStore.AddLicenseAsync(licenseDocument, cancellationToken).ConfigureAwait(false);
            }

            return this.Ok();
        }

        /// <summary>
        /// Gets the current license.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [Route("api/license")]
        public async Task<ActionResult> GetLicenseAsync(CancellationToken cancellationToken)
        {
            var license = await this.licenseStore.GetLicenseAsync(cancellationToken).ConfigureAwait(false);

            if (license == null)
            {
                return this.NotFound();
            }

            return new FileContentResult(Encoding.UTF8.GetBytes(license.ToString()), "application/xml");
        }
    }
}
