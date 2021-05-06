// <copyright file="DeveloperDisk.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft;
using Nerdbank.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// Represents an individual developer disk.
    /// </summary>
    public class DeveloperDisk : IDisposableObservable
    {
        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the version information embedded in the developer disk.
        /// </summary>
        public SystemVersion Version { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Stream"/> which provides access to the developer disk image.
        /// </summary>
        public Stream Image { get; set; }

        /// <summary>
        /// Gets or sets the signature of the developer disk.
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Creates a <see cref="DeveloperDisk"/> object based on a file.
        /// </summary>
        /// <param name="image">
        /// A <see cref="Stream"/> which represents the developer disk image.
        /// </param>
        /// <param name="signature">
        /// A <see cref="Stream"/> which represents the developer disk signature.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and, when completed,
        /// returns a <see cref="DeveloperDisk"/> object which represents the developer disk.
        /// </returns>
        public static async Task<DeveloperDisk> FromFileAsync(Stream image, Stream signature, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            var version = DeveloperDiskReader.GetVersionInformation(image);
            image.Seek(0, SeekOrigin.Begin);

            byte[] signatureBytes = new byte[signature.Length];
            signature.Seek(0, SeekOrigin.Begin);
            await signature.ReadBlockOrThrowAsync(signatureBytes, cancellationToken).ConfigureAwait(false);

            return new DeveloperDisk()
            {
                Image = image,
                Signature = signatureBytes,
                Version = version,
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Image?.Dispose();
            this.IsDisposed = true;
        }
    }
}
