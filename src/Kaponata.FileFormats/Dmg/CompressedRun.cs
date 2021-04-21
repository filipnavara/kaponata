// <copyright file="CompressedRun.cs" company="Kenneth Bell, Quamotion bv">
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

using DiscUtils.Streams;
using System;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Represents an individual compressed run in a <see cref="CompressedBlock"/> entry.
    /// </summary>
    internal class CompressedRun : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the length of the compressed data of this <see cref="CompressedRun"/>,
        /// expressed in bytes.
        /// </summary>
        public long CompLength { get; set; }

        /// <summary>
        /// Gets or sets the offset of this <see cref="CompressedRun"/>, relative to the start
        /// of the data fork.
        /// </summary>
        public long CompOffset { get; set; }

        /// <summary>
        /// Gets or sets length of the decompresed data of this <see cref="CompressedRun"/>, expressed
        /// in sectors.
        /// </summary>
        public long SectorCount { get; set; }

        /// <summary>
        /// Gets or sets the number of the first sector in this <see cref="CompressedRun"/>.
        /// </summary>
        public long SectorStart { get; set; }

        /// <summary>
        /// Gets or sets the compression type used for this <see cref="CompressedRun"/>.
        /// </summary>
        public RunType Type { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return 40; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Type = (RunType)EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.SectorStart = EndianUtilities.ToInt64BigEndian(buffer, offset + 8);
            this.SectorCount = EndianUtilities.ToInt64BigEndian(buffer, offset + 16);
            this.CompOffset = EndianUtilities.ToInt64BigEndian(buffer, offset + 24);
            this.CompLength = EndianUtilities.ToInt64BigEndian(buffer, offset + 32);

            return 40;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}