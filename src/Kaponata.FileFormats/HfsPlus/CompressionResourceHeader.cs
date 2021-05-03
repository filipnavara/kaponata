// <copyright file="CompressionResourceHeader.cs" company="Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// The header for compression data stored in the catalog file.
    /// </summary>
    public class CompressionResourceHeader : IByteArraySerializable
    {
        /// <inheritdoc/>
        public int Size
        {
            get { return 16; }
        }

        /// <summary>
        /// Gets the size of all data blocks.
        /// </summary>
        public uint DataSize { get; private set; }

        /// <summary>
        /// Gets additional flags.
        /// </summary>
        public uint Flags { get; private set; }

        /// <summary>
        /// Gets the size of the header.
        /// </summary>
        public uint HeaderSize { get; private set; }

        /// <summary>
        /// Gets the total size. This is the sum of <see cref="DataSize"/> and <see cref="HeaderSize"/>.
        /// </summary>
        public uint TotalSize { get; private set; }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.HeaderSize = EndianUtilities.ToUInt32BigEndian(buffer, offset);
            this.TotalSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.DataSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.Flags = EndianUtilities.ToUInt32BigEndian(buffer, offset + 12);

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}