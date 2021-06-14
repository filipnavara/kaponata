// <copyright file="DeveloperDiskController.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperDisks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// Provides methods for uploading developer disk images to the cluster.
    /// </summary>
    public class DeveloperDiskController : Controller
    {
        private readonly DeveloperDiskStore store;
        private readonly DeveloperDiskFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperDiskController"/> class.
        /// </summary>
        /// <param name="store">
        /// A <see cref="DeveloperDiskStore"/> which stores the developer disk images.
        /// </param>
        /// <param name="factory">
        /// A <see cref="DeveloperDiskFactory"/> which can be used to read developer disk images.
        /// </param>
        public DeveloperDiskController(DeveloperDiskStore store, DeveloperDiskFactory factory)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Imports a new developer disk, together with its signature.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Route("api/ios/developerDisk")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<ActionResult> ImportDeveloperDiskAsync(CancellationToken cancellationToken)
        {
            // We only accept form data.
            if (string.IsNullOrEmpty(this.Request.ContentType) ||
               this.Request.ContentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) != 0
                || this.Request.Form == null)
            {
                return this.StatusCode((int)HttpStatusCode.UnsupportedMediaType);
            }

            // We expect a DeveloperDiskImage.dmg and DeveloperDiskImage.dmg.signature file
            if (this.Request.Form.Files.Count(f => f.FileName == "DeveloperDiskImage.dmg") != 1)
            {
                return this.BadRequest("You must specify exactly one DeveloperDiskImage.dmg file");
            }

            if (this.Request.Form.Files.Count(f => f.FileName == "DeveloperDiskImage.dmg.signature") != 1)
            {
                return this.BadRequest("You must specify exactly one DeveloperDiskImage.dmg.signature file");
            }

            var developerDiskImageFile = this.Request.Form.Files.Single(f => f.FileName == "DeveloperDiskImage.dmg");
            var developerDiskImageSignatureFile = this.Request.Form.Files.Single(f => f.FileName == "DeveloperDiskImage.dmg.signature");

            if (developerDiskImageFile.Length < 4 * 1024 * 1024)
            {
                return this.BadRequest("The developer disk image is less than 4 MB in size, and is likely corrupt");
            }

            if (developerDiskImageSignatureFile.Length == 0)
            {
                return this.BadRequest("The developer disk image signature is an empty file, and is likely corrupt.");
            }

            if (developerDiskImageSignatureFile.Length > 0x100)
            {
                return this.BadRequest("The developer disk image signature is larger than 4096 bytes, and is likely corrupt.");
            }

            using (var signatureStream = developerDiskImageSignatureFile.OpenReadStream())
            using (var developerDiskImageStream = new TempFileStream())
            {
                // Fetch information from the DeveloperDiskImage.dmg file (especially the target iOS version),
                // and then copy the files to disk.
                await developerDiskImageFile.CopyToAsync(developerDiskImageStream, cancellationToken).ConfigureAwait(false);
                developerDiskImageStream.Position = 0;

                var disk = await this.factory.FromFileAsync(developerDiskImageStream, signatureStream, cancellationToken).ConfigureAwait(false);
                await this.store.AddAsync(disk, cancellationToken).ConfigureAwait(false);
            }

            return this.Ok();
        }

        /// <summary>
        /// Asynchronously lists all developer disk images which are available.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Route("api/ios/developerDisks")]
        public async Task<JsonResult> ListDeveloperDisksAsync(CancellationToken cancellationToken)
        {
            var versions = await this.store.ListAsync(cancellationToken).ConfigureAwait(false);
            return new JsonResult(versions.Select(v => v.ToString()).ToList());
        }

        /// <summary>
        /// Downloads a developer disk image.
        /// </summary>
        /// <param name="version">
        /// The iOS version for which to download the developer disk image.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// An action which allows you to download the developer disk image.
        /// </returns>
        [Route("api/ios/developerDisk/{version}/disk")]
        [HttpGet]
        public async Task<ActionResult> DownloadDeveloperDiskAsync(string version, CancellationToken cancellationToken)
        {
            if (!Version.TryParse(version, out Version? parsedVersion))
            {
                return this.NotFound();
            }

            var disk = await this.store.GetAsync(parsedVersion, cancellationToken);

            if (disk == null)
            {
                return this.NotFound();
            }

            return new FileStreamResult(disk.Image, "application/octet-stream")
            {
                FileDownloadName = "DeveloperDiskImage.dmg",
            };
        }

        /// <summary>
        /// Downloads the signature of a developer disk image.
        /// </summary>
        /// <param name="version">
        /// The iOS version for which to download the developer disk image.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// An action which allows you to download the developer disk image signature.
        /// </returns>
        [Route("api/ios/developerDisk/{version}/signature")]
        [HttpGet]
        public async Task<ActionResult> DownloadDeveloperDiskSignatureAsync(string version, CancellationToken cancellationToken)
        {
            if (!Version.TryParse(version, out Version? parsedVersion))
            {
                return this.NotFound();
            }

            var disk = await this.store.GetAsync(parsedVersion, cancellationToken);

            if (disk == null)
            {
                return this.NotFound();
            }

            return new FileContentResult(disk.Signature, "application/octet-stream")
            {
                FileDownloadName = "DeveloperDiskImage.dmg.signature",
            };
        }
    }
}
