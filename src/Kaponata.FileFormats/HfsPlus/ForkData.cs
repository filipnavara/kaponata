// <copyright file="ForkData.cs" company="Kenneth Bell, Quamotion bv">
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

#nullable disable

using DiscUtils.Streams;
using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents the HFS+ Fork Data structure.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#ForkDataStructure"/>
    internal sealed class ForkData : IByteArraySerializable
    {
        private const int StructSize = 80;
        private uint clumpSize;

        /// <summary>
        /// Gets or sets an array of extent descriptors for the fork.
        /// </summary>
        public ExtentDescriptor[] Extents { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the valid data in the fork.
        /// </summary>
        public ulong LogicalSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of allocation blocks used by all the extents in this fork.
        /// </summary>
        public uint TotalBlocks { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return StructSize; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.LogicalSize = EndianUtilities.ToUInt64BigEndian(buffer, offset + 0);
            this.clumpSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.TotalBlocks = EndianUtilities.ToUInt32BigEndian(buffer, offset + 12);

            this.Extents = new ExtentDescriptor[8];
            for (int i = 0; i < 8; ++i)
            {
                this.Extents[i] = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 16 + (i * 8));
            }

            return StructSize;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}