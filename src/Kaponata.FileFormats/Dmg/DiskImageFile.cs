// <copyright file="DiskImageFile.cs" company="Kenneth Bell, Quamotion bv">
// Copyright (c) 2008-2011, Kenneth Bell
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Claunia.PropertyList;
using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// A <see cref="VirtualDiskLayer"/> wich provides access to a DMG file.
    /// </summary>
    public sealed class DiskImageFile : VirtualDiskLayer
    {
        private readonly Ownership ownsStream;
        private readonly ResourceFork resources;
        private readonly UdifResourceFile udifHeader;
        private Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskImageFile"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="ownsStream">Indicates if the new instance should control the lifetime of the stream.</param>
        public DiskImageFile(Stream stream, Ownership ownsStream)
        {
            this.udifHeader = new UdifResourceFile();
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.ownsStream = ownsStream;

            if (stream.Length < this.udifHeader.Size)
            {
                throw new InvalidDataException("The file is not a valid DMG file: could not read the UDIF header.");
            }

            stream.Position = stream.Length - this.udifHeader.Size;
            byte[] data = StreamUtilities.ReadExact(stream, this.udifHeader.Size);

            this.udifHeader.ReadFrom(data, 0);

            if (!this.udifHeader.SignatureValid)
            {
                throw new InvalidDataException("The file is not a valid DMG file: could not read the UDIF header.");
            }

            stream.Position = (long)this.udifHeader.XmlOffset;
            byte[] xmlData = StreamUtilities.ReadExact(stream, (int)this.udifHeader.XmlLength);
            Dictionary<string, object> plist = (Dictionary<string, object>)XmlPropertyListParser.Parse(xmlData).ToObject();

            this.resources = ResourceFork.FromPlist(plist);
            this.Buffer = new UdifBuffer(stream, this.resources, this.udifHeader.SectorCount);
        }

        /// <summary>
        /// Gets the <see cref="UdifBuffer"/> which enables access to the data stored on the disk.
        /// </summary>
        public UdifBuffer Buffer { get; }

        /// <inheritdoc/>
        public override long Capacity
        {
            get { return this.Buffer.Capacity; }
        }

        /// <summary>
        /// Gets the geometry of the virtual disk layer.
        /// </summary>
        public override Geometry Geometry
        {
            get { return Geometry.FromCapacity(this.Capacity); }
        }

        /// <inheritdoc/>
        public override bool IsSparse
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the file is a differencing disk.
        /// </summary>
        public override bool NeedsParent
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override FileLocator RelativeFileLocator
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public override SparseStream OpenContent(SparseStream parentStream, Ownership ownsStream)
        {
            if (parentStream != null)
            {
                // The origina implementation would dispose of the parent stream when not null and ownsStream is
                // set. This use case seems to never occur; so just throw instead.
                throw new ArgumentException("DiskImageFile.OpenContent does not support the parentStream parameter");
            }

            SparseStream rawStream = new BufferStream(this.Buffer, FileAccess.Read);
            return new BlockCacheStream(rawStream, Ownership.Dispose);
        }

        /// <summary>
        /// Gets the location of the parent file, given a base path.
        /// </summary>
        /// <returns>Array of candidate file locations.</returns>
        public override string[] GetParentLocations()
        {
            return Array.Empty<string>();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.stream != null && this.ownsStream == Ownership.Dispose)
                {
                    this.stream.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}