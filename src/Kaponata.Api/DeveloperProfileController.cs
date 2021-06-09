// <copyright file="DeveloperProfileController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// A controller which allows you to interact with the iOS developer profile package. Supports CRD (no update) operations
    /// for certificates and provisioning profiles.
    /// </summary>
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None)]
    public class DeveloperProfileController : Controller
    {
        private readonly KubernetesDeveloperProfile developerProfile;
        private readonly ILogger<DeveloperProfileController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperProfileController"/> class.
        /// </summary>
        /// <param name="developerProfile">
        /// The <see cref="KubernetesDeveloperProfile"/> used to store the individual developer profile data.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public DeveloperProfileController(KubernetesDeveloperProfile developerProfile, ILogger<DeveloperProfileController> logger)
        {
            this.developerProfile = developerProfile ?? throw new ArgumentNullException(nameof(developerProfile));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously imports a developer profile into Kaponata.
        /// </summary>
        /// <param name="developerProfile">
        /// The developer profile to import.
        /// </param>
        /// <param name="password">
        /// The password used to protect the developer profile.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        [Route("api/ios/developerProfile")]
        [HttpPost]
        public async Task<ActionResult> ImportDeveloperProfileAsync(IFormFile developerProfile, [FromForm] string password, CancellationToken cancellationToken)
        {
            using (var developerProfileStream = developerProfile.OpenReadStream())
            using (var profile = await DeveloperProfilePackage.ReadAsync(developerProfileStream, password, cancellationToken).ConfigureAwait(false))
            {
                foreach (var identity in profile.Identities)
                {
                    await this.developerProfile.AddCertificateAsync(identity, cancellationToken).ConfigureAwait(false);
                }

                foreach (var provisioningProfile in profile.ProvisioningProfiles)
                {
                    await this.developerProfile.AddProvisioningProfileAsync(provisioningProfile, cancellationToken).ConfigureAwait(false);
                }
            }

            return this.Ok();
        }

        /// <summary>
        /// Asynchronously exports the developer profile stored in Kaponata.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        [Route("api/ios/developerProfile")]
        [HttpGet]
        public async Task<ActionResult> GetDeveloperProfileAsync(CancellationToken cancellationToken)
        {
            using (var developerProfilePackage = new DeveloperProfilePackage())
            {
                foreach (var certificate in await this.developerProfile.GetCertificatesAsync(cancellationToken))
                {
                    developerProfilePackage.Identities.Add(certificate);
                }

                foreach (var provisioningProfile in await this.developerProfile.GetProvisioningProfilesAsync(cancellationToken))
                {
                    developerProfilePackage.ProvisioningProfiles.Add(provisioningProfile);
                }

                if (developerProfilePackage.Identities.Count == 0 && developerProfilePackage.ProvisioningProfiles.Count == 0)
                {
                    return this.NotFound();
                }

                MemoryStream memoryStream = new MemoryStream();
                await developerProfilePackage.WriteAsync(memoryStream, "kaponata", cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;

                return this.File(memoryStream, contentType: "application/octet-stream", fileDownloadName: "kaponata.developerprofile");
            }
        }
    }
}