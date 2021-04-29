// <copyright file="XipMetadata.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;

namespace Kaponata.FileFormats.Xip
{
    /// <summary>
    /// The metadata embedded in a <see cref="XipFile"/>.
    /// </summary>
    public class XipMetadata
    {
        /// <summary>
        /// Gets or sets the size of the data embedded in this <see cref="XipFile"/>, when fully decompressed.
        /// </summary>
        public long UncompressedSize { get; set; }

        /// <summary>
        /// Gets or sets the version of the Xip file format used. Always 1.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the compression format in use. Can be <see langword="null"/> (in which case the content
        /// is gzip-compressed) or <c>10.10</c>, in which bpzx-compression is used.
        /// </summary>
        public string? FileSystemCompressionFormat { get; set; }

        /// <summary>
        /// Populates this <see cref="XipMetadata"/> with data from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="NSDictionary"/> from which to read data.
        /// </param>
        public void ReadFrom(NSDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            this.UncompressedSize = ((NSNumber)dictionary[nameof(this.UncompressedSize)]).ToLong();
            this.Version = (int)dictionary[nameof(this.Version)].ToObject();

            if (dictionary.ContainsKey(nameof(this.FileSystemCompressionFormat)))
            {
                this.FileSystemCompressionFormat = (string)dictionary[nameof(this.FileSystemCompressionFormat)].ToObject();
            }
        }
    }
}
