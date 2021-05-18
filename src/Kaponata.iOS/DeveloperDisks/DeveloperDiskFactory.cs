// <copyright file="DeveloperDiskFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Nerdbank.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// Supports creating <see cref="DeveloperDisk"/> objects based on <see cref="Stream"/>s.
    /// </summary>
    public class DeveloperDiskFactory
    {
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
        public virtual async Task<DeveloperDisk> FromFileAsync(Stream image, Stream signature, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            (var version, var creationTime) = DeveloperDiskReader.GetVersionInformation(image);
            image.Seek(0, SeekOrigin.Begin);

            byte[] signatureBytes = new byte[signature.Length];
            signature.Seek(0, SeekOrigin.Begin);
            await signature.ReadBlockOrThrowAsync(signatureBytes, cancellationToken).ConfigureAwait(false);

            return new DeveloperDisk()
            {
                Image = image,
                Signature = signatureBytes,
                Version = version,
                CreationTime = creationTime,
            };
        }
    }
}
