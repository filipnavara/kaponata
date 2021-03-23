// <copyright file="ProvisioningProfileController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// REST methods for managing provisioning profiles.
    /// </summary>
    public partial class ProvisioningProfileController : Controller
    {
        private readonly KubernetesDeveloperProfile developerProfile;
        private readonly ILogger<ProvisioningProfileController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvisioningProfileController"/> class.
        /// </summary>
        /// <param name="developerProfile">
        /// A <see cref="KubernetesDeveloperProfile"/> class which is used to store the provisioning profiles in the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        public ProvisioningProfileController(KubernetesDeveloperProfile developerProfile, ILogger<ProvisioningProfileController> logger)
        {
            this.developerProfile = developerProfile ?? throw new ArgumentNullException(nameof(developerProfile));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a provisioning profile.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ActionResult"/> describing the status of the operation.
        /// </returns>
        [Route("api/ios/provisioningProfiles")]
        [HttpPost]
        public async Task<ActionResult> AddProvisioningProfileAsync(CancellationToken cancellationToken)
        {
            if (this.Request.ContentLength == null)
            {
                return this.BadRequest();
            }

            int length = (int)this.Request.ContentLength;

            using (var memory = MemoryPool<byte>.Shared.Rent(length))
            {
                var buffer = memory.Memory.Slice(0, length);
                await this.Request.Body.ReadBlockOrThrowAsync(buffer, cancellationToken).ConfigureAwait(false);

                SignedCms cms = new SignedCms();
                cms.Decode(buffer.Span);

                var profile = await this.developerProfile.AddProvisioningProfileAsync(cms, cancellationToken).ConfigureAwait(false);

                return this.Ok(profile);
            }
        }

        /// <summary>
        /// Gets a list of POCOs describing all provisioning profiles which are currently available.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A list of POCos describing all provisioning profiles which are currently available.
        /// </returns>
        [Route("api/ios/provisioningProfiles")]
        public async Task<ActionResult> GetProvisioningProfilesAsync(CancellationToken cancellationToken)
        {
            var provisioningProfiles = await this.developerProfile.GetProvisioningProfilesAsync(cancellationToken).ConfigureAwait(false);
            var summaries = provisioningProfiles.Select(p => ProvisioningProfile.Read(p)).ToList();

            return this.Ok(summaries);
        }

        /// <summary>
        /// Gets an individual provisioning profile as <c>.mobileprovision</c> file.
        /// </summary>
        /// <param name="uuid">
        /// The UUID of the provisioning profile to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <c>.mobileprovision</c> file.
        /// </returns>
        [Route("api/ios/provisioningProfiles/{uuid}")]
        public async Task<ActionResult> GetProvisioningProfileAsync(Guid uuid, CancellationToken cancellationToken)
        {
            var profile = await this.developerProfile.GetProvisioningProfileAsync(uuid, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return this.NotFound();
            }
            else
            {
                return this.File(profile.Encode(), contentType: "application/octet-stream", fileDownloadName: $"{uuid}.mobileprovision");
            }
        }

        /// <summary>
        /// Deletes a provisioning profile.
        /// </summary>
        /// <param name="uuid">
        /// The UUID of the provisioning profile to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ActionResult"/> describing the status of the operation.
        /// </returns>
        [Route("api/ios/provisioningProfiles/{uuid}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProvisioningProfileAsync(Guid uuid, CancellationToken cancellationToken)
        {
            if (await this.developerProfile.DeleteProvisioningProfileAsync(uuid, cancellationToken).ConfigureAwait(false))
            {
                return this.Ok();
            }
            else
            {
                return this.NotFound();
            }
        }
    }
}
