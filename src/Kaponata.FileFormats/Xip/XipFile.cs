// <copyright file="XipFile.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using DiscUtils.Streams;
using Kaponata.FileFormats.Cpio;
using Kaponata.FileFormats.Xar;
using Microsoft;
using System;
using System.IO;
using System.IO.Compression;

namespace Kaponata.FileFormats.Xip
{
    /// <summary>
    /// An .XIP file is a XAR archive which can be digitally signed for integrity. It usually contains two files:
    /// a <see cref="Metadata"/> file and a <see cref="CpioFile"/> which is stored as a <c>Content</c> file.
    /// </summary>
    public class XipFile : IDisposable, IDisposableObservable
    {
        private readonly Stream stream;
        private readonly Ownership ownership;
        private readonly XarFile xarFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="XipFile"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which provides access to the xip file.
        /// </param>
        /// <param name="ownership">
        /// A value indicating the ownership of the <paramref name="stream"/>.
        /// </param>
        public XipFile(Stream stream, Ownership ownership = Ownership.Dispose)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.xarFile = new XarFile(this.stream, leaveOpen: true);
            this.ownership = ownership;
        }

        /// <summary>
        /// Gets the <see cref="XipMetadata"/> for this file.
        /// This property is populated after you call <see cref="Open"/>.
        /// </summary>
        public XipMetadata Metadata { get; } = new XipMetadata();

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Opens the contents of this <see cref="XipFile"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="CpioFile"/> which represents the CPIO file embedded in this
        /// <see cref="XipFile"/>.
        /// </returns>
        public CpioFile Open()
        {
            Verify.NotDisposed(this);

            using (var metadataStream = this.xarFile.Open("Metadata"))
            {
                var metadataDictionary = (NSDictionary)XmlPropertyListParser.Parse(metadataStream);
                this.Metadata.ReadFrom(metadataDictionary);
            }

            var content = this.xarFile.Open("Content");

            // The .xip file will use the 'pbzx' compression method if
            // FileSystemCompressionFormat == "10.10". This isn't implemented (yet).
            if (this.Metadata.FileSystemCompressionFormat == null)
            {
                var gz = new GZipStream(content, CompressionMode.Decompress, leaveOpen: false);
                return new CpioFile(gz, leaveOpen: false);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.ownership == Ownership.Dispose)
            {
                this.stream.Dispose();
            }

            this.xarFile.Dispose();

            this.IsDisposed = true;
        }
    }
}
