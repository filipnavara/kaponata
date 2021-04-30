//-----------------------------------------------------------------------
// <copyright file="XarFile.cs" company="Quamotion bv">
//     Copyright (c) 2016 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using DiscUtils.Compression;
using DiscUtils.Streams;
using Nerdbank.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Kaponata.FileFormats.Xar
{
    /// <summary>
    /// Xar (Extensible ARchives) is a compression file format used by Apple.
    /// The file consists of three sections, the header, the table of contents and the heap.
    /// The header is a normal C struct, the table of contents is a zlib-compressed XML
    /// document and the heap contains the compressed data.
    /// </summary>
    /// <seealso href="https://github.com/mackyle/xar/wiki/xarformat"/>
    public unsafe class XarFile : IDisposable
    {
        /// <summary>
        /// The <see cref="Stream"/> around which this <see cref="XarFile"/> wraps.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Indicates whether to close <see cref="stream"/> when we are disposed of.
        /// </summary>
        private readonly bool leaveOpen;

        /// <summary>
        /// The table of contents, listing the entries compressed in this archive.
        /// </summary>
        private readonly XDocument toc;

        /// <summary>
        /// The entries contained in the table of contents.
        /// </summary>
        private readonly List<XarFileEntry> files;

        /// <summary>
        /// The start of the heap.
        /// </summary>
        private readonly ulong heapStart;

        /// <summary>
        /// Initializes a new instance of the <see cref="XarFile"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> which represents the XAR archive.
        /// </param>
        /// <param name="leaveOpen">
        /// Indicates whether to close <paramref name="stream"/> when the <see cref="XarFile"/>
        /// is disposed of.
        /// </param>
        public XarFile(Stream stream, bool leaveOpen)
        {
            this.leaveOpen = leaveOpen;
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

            XarHeader header = default;
            var buffer = new byte[((IByteArraySerializable)header).Size];
            stream.Read(buffer, 0, buffer.Length);
            header.ReadFrom(buffer, 0);

            // Basic validation
            if (header.Signature != XarHeader.Magic)
            {
                throw new InvalidDataException("The XAR header signature is incorrect");
            }

            if (header.Version != XarHeader.CurrentVersion)
            {
                throw new InvalidDataException("The XAR header version is incorrect");
            }

            // Read the digest name, if available.
            int messageDigestNameLength = header.Size - 28;
            Span<byte> messageDigestNameBytes = stackalloc byte[messageDigestNameLength];
            stream.Read(messageDigestNameBytes);
            string messageDigestName = Encoding.UTF8.GetString(messageDigestNameBytes);

            // Read the table of contents
            using (var compressedTocStream = this.stream.ReadSlice((long)header.TocLengthCompressed))
            using (var decompressedTocStream = new ZlibStream(compressedTocStream, CompressionMode.Decompress, leaveOpen: true))
            {
                // Read the TOC
                this.toc = XDocument.Load(decompressedTocStream);
                this.files = (from file
                              in this.toc!
                                 .Element("xar") !
                                 .Element("toc") !
                                 .Elements("file")
                              select new XarFileEntry(file)).ToList();
            }

            this.heapStart = header.Size + header.TocLengthCompressed;
        }

        /// <summary>
        /// Gets a list of all files embedded in this <see cref="XarFile"/>.
        /// </summary>
        public List<XarFileEntry> Files => this.files;

        /// <summary>
        /// Gets the names of all entries in this <see cref="XarFile"/>.
        /// </summary>
        public List<string> EntryNames => this.GetEntryNames();

        /// <summary>
        /// Opens an entry in the <see cref="XarFile"/>.
        /// </summary>
        /// <param name="entryName">
        /// The name of the entry to open.
        /// </param>
        /// <returns>
        /// A <see cref="Stream"/> which represents the entry.
        /// </returns>
        public Stream Open(string entryName)
        {
            if (entryName == null)
            {
                throw new ArgumentNullException(nameof(entryName));
            }

            // Fetch the entry detail
            IEnumerable<XarFileEntry> files = this.files;
            XarFileEntry? entry = null;

            foreach (var part in entryName.Split('/'))
            {
                entry = files.Where(f => f.Name == part).SingleOrDefault();

                if (entry == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(entryName));
                }

                files = entry.Files;
            }

            Debug.Assert(entry != null, "The requested entry was not found");

            if (entry.Type != XarEntryType.File)
            {
                throw new ArgumentOutOfRangeException(nameof(entryName));
            }

            // Construct a substream which maps to the compressed data
            var start = (long)this.heapStart + entry.DataOffset;

            this.stream.Seek(start, SeekOrigin.Begin);
            var substream = this.stream.ReadSlice((long)entry.DataLength);

            // Special case: uncompressed data can be returned 'as is'
            if (entry.Encoding == "application/octet-stream")
            {
                return substream;
            }
            else if (entry.Encoding == "application/x-gzip")
            {
                // Create a new deflate stream, and return it.
                return new ZlibStream(substream, CompressionMode.Decompress, leaveOpen: false);
            }
            else if (entry.Encoding == "application/x-bzip2")
            {
                return new BZip2DecoderStream(substream, Ownership.Dispose);
            }
            else
            {
                throw new InvalidDataException("Only gzip-compressed data is supported");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.stream.Dispose();
            }
        }

        private List<string> GetEntryNames()
        {
            List<string> values = new List<string>();

            foreach (var file in this.files)
            {
                this.GetEntryNames(null, file, values);
            }

            return values;
        }

        private void GetEntryNames(string? path, XarFileEntry file, List<string> target)
        {
            var fullName = path == null ? file.Name : $"{path}/{file.Name}";

            target.Add(fullName);

            foreach (var child in file.Files)
            {
                this.GetEntryNames(fullName, child, target);
            }
        }
    }
}
