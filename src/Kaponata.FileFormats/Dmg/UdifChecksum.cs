// <copyright file="UdifChecksum.cs" company="Kenneth Bell, Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// Copyright (c) 2008-2011, Kenneth Bell
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

#nullable disable

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Represents a checksum found in DMG files.
    /// </summary>
    /// <seealso href="http://newosxbook.com/DMG.html"/>
    internal class UdifChecksum : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the size of the checksum.
        /// </summary>
        public uint ChecksumSize { get; set; }

        /// <summary>
        /// Gets or sets the actual checksum data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the type of data being checked (e.g. data fork
        /// or master).
        /// </summary>
        public uint Type { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return 136; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Type = EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.ChecksumSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.Data = EndianUtilities.ToByteArray(buffer, offset + 8, 128);

            return 136;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}