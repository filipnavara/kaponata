﻿// <copyright file="CompressionResourceBlockHead.cs" company="Quamotion bv">
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
    /// The haeder for an individual compression resource block.
    /// </summary>
    internal class CompressionResourceBlockHead : IByteArraySerializable
    {
        /// <inheritdoc/>
        public int Size
        {
            get { return 8; }
        }

        /// <summary>
        /// Gets the total data size.
        /// </summary>
        public uint DataSize { get; private set; }

        /// <summary>
        /// Gets the number of blocks.
        /// </summary>
        public uint NumBlocks { get; private set; }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.DataSize = EndianUtilities.ToUInt32BigEndian(buffer, offset);
            this.NumBlocks = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 4);

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}