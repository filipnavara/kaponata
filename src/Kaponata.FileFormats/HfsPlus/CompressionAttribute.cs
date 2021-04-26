// <copyright file="CompressionAttribute.cs" company="Kenneth Bell, Quamotion bv">
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
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents the data embedded in the in-line <c>decmpfs</c> HFS+ xattr.
    /// </summary>
    internal class CompressionAttribute : IByteArraySerializable
    {
        /// <summary>
        /// The magic used to identify the attribute.
        /// </summary>
        public const uint DecmpfsMagic = 0x66706d63; /* cmpf */

        /// <inheritdoc/>
        public int Size => 16;

        /// <summary>
        /// Gets or sets the magic of this attribute. Should be <see cref="DecmpfsMagic"/>.
        /// </summary>
        public uint CompressionMagic { get; set; }

        /// <summary>
        /// Gets or sets the type of compression used.
        /// </summary>
        public FileCompressionType CompressionType { get; set; }

        /// <summary>
        /// Gets or sets the size of the uncompressed file.
        /// </summary>
        public ulong UncompressedSize { get; set; }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.CompressionMagic = EndianUtilities.ToUInt32BigEndian(buffer, offset);

            if (this.CompressionMagic != DecmpfsMagic)
            {
                throw new InvalidDataException();
            }

            this.CompressionType = (FileCompressionType)EndianUtilities.ToUInt32LittleEndian(buffer, offset + 4);
            this.UncompressedSize = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 8);

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}