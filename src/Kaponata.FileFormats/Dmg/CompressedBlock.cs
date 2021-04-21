//
// Copyright (c) 2008-2011, Kenneth Bell
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
//

#nullable disable

using DiscUtils.Streams;
using System;
using System.Collections.Generic;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Contains information about a block of compressed data in a DMG file. Also known
    /// as a <c>blkx</c> descriptor. A compressed block usually maps to an entire partition.
    /// </summary>
    /// <seealso href="http://newosxbook.com/DMG.html"/>
    internal class CompressedBlock : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the number of block descriptors.
        /// </summary>
        public uint BlocksDescriptor { get; set; }

        /// <summary>
        /// Gets or sets a checksum for the current block.
        /// </summary>
        public UdifChecksum CheckSum { get; set; }

        /// <summary>
        /// Gets or sets the offset at which the data starts.
        /// </summary>
        public ulong DataStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether decompress buffers are required
        /// when processing this block.
        /// </summary>
        public uint DecompressBufferRequested { get; set; }

        /// <summary>
        /// Gets or sets the starting disk sector in this <c>blkx</c> descriptor.
        /// </summary>
        public long FirstSector { get; set; }

        /// <summary>
        /// Gets or sets the version number of this header.
        /// </summary>
        public uint InfoVersion { get; set; }

        /// <summary>
        /// Gets or sets a list containing all individual run entries in this block.
        /// </summary>
        public List<CompressedRun> Runs { get; set; }

        /// <summary>
        /// Gets or sets the total number of disk sectors in this <c>blkx</c> descriptor.
        /// </summary>
        public long SectorCount { get; set; }

        /// <summary>
        /// Gets or sets the magic of this compressed block. Always <c>mish</c>.
        /// </summary>
        public uint Signature { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Signature = EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.InfoVersion = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.FirstSector = EndianUtilities.ToInt64BigEndian(buffer, offset + 8);
            this.SectorCount = EndianUtilities.ToInt64BigEndian(buffer, offset + 16);
            this.DataStart = EndianUtilities.ToUInt64BigEndian(buffer, offset + 24);
            this.DecompressBufferRequested = EndianUtilities.ToUInt32BigEndian(buffer, offset + 32);
            this.BlocksDescriptor = EndianUtilities.ToUInt32BigEndian(buffer, offset + 36);

            this.CheckSum = EndianUtilities.ToStruct<UdifChecksum>(buffer, offset + 60);

            this.Runs = new List<CompressedRun>();
            int numRuns = EndianUtilities.ToInt32BigEndian(buffer, offset + 200);
            for (int i = 0; i < numRuns; ++i)
            {
                this.Runs.Add(EndianUtilities.ToStruct<CompressedRun>(buffer, offset + 204 + (i * 40)));
            }

            return 0;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}