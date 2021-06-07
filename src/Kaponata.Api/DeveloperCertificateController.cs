// <copyright file="DeveloperCertificateController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// Supports manageing developer certificates (or identities) used by Kaponata for iOS test automation .
    /// </summary>
    public class DeveloperCertificateController : Controller
    {
        private readonly KubernetesDeveloperProfile developerProfile;
        private readonly ILogger<DeveloperCertificateController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperCertificateController"/> class.
        /// </summary>
        /// <param name="developerProfile">
        /// A <see cref="KubernetesDeveloperProfile"/> class which is used to store the provisioning profiles in the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        public DeveloperCertificateController(KubernetesDeveloperProfile developerProfile, ILogger<DeveloperCertificateController> logger)
        {
            this.developerProfile = developerProfile ?? throw new ArgumentNullException(nameof(developerProfile));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a certificate and its private key to the cluster.
        /// </summary>
        /// <param name="certificate">
        /// The X.509 certificate and its associated private key, as a PKCS#12 file, to add.
        /// </param>
        /// <param name="password">
        /// The password used to protect the private key.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ActionResult"/> describing the status of the operation.
        /// </returns>
        [Route("kaponata/ios/identities")]
        [HttpPost]
        public async Task<ActionResult> AddCertificateAsync([FromForm] IFormFile certificate, [FromForm] string password, CancellationToken cancellationToken)
        {
            if (certificate == null || password == null)
            {
                return this.BadRequest("You must upload both the certificate and password as form data.");
            }

            byte[] data = new byte[certificate.Length];
            using (Stream certificateStream = certificate.OpenReadStream())
            {
                await certificateStream.ReadBlockAsync(data, cancellationToken).ConfigureAwait(false);
            }

            using (var x509Certificate = new X509Certificate2(data, password))
            {
                await this.developerProfile.AddCertificateAsync(x509Certificate, cancellationToken).ConfigureAwait(false);
            }

            return this.Ok();
        }

        /// <summary>
        /// Lists all certificates embedded in the developer profile.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A list of POCOs with information of the certificates.
        /// </returns>
        [Route("kaponata/ios/identities")]
        public async Task<ActionResult> GetCertificatesAsync(CancellationToken cancellationToken)
        {
            var certificates = await this.developerProfile.GetCertificatesAsync(cancellationToken).ConfigureAwait(false);
            var summaries = certificates.Select(c => Identity.FromX509Certificate(c)).ToList();

            return this.Ok(summaries);
        }

        /// <summary>
        /// Gets a certificate as a <c>.cer</c> file. Does not include the private key.
        /// </summary>
        /// <param name="thumbprint">
        /// The thumbprint of the certificate to get.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The certificate as a <c>.cer</c> file.
        /// </returns>
        [Route("kaponata/ios/identities/{thumbprint}")]
        public async Task<ActionResult> GetCertificateAsync(string thumbprint, CancellationToken cancellationToken)
        {
            var certificate = await this.developerProfile.GetCertificateAsync(thumbprint, cancellationToken).ConfigureAwait(false);

            if (certificate == null)
            {
                return this.NotFound();
            }
            else
            {
                return this.File(
                    fileContents: certificate.Export(X509ContentType.Cert),
                    contentType: "application/x-x509-user-cert",
                    fileDownloadName: $"{thumbprint}.cer");
            }
        }

        /// <summary>
        /// Deletes a certificate.
        /// </summary>
        /// <param name="thumbprint">
        /// The thumbprint of the certiifcate to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="ActionResult"/> describing the status of the operation.
        /// </returns>
        [Route("kaponata/ios/identities/{thumbprint}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCertificateAsync(string thumbprint, CancellationToken cancellationToken)
        {
            try
            {
                await this.developerProfile.DeleteCertificateAsync(thumbprint, cancellationToken).ConfigureAwait(false);

                return this.Ok();
            }
            catch (InvalidOperationException)
            {
                return this.NotFound();
            }
        }
    }
}
