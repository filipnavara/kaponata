// <copyright file="CatalogDirInfo.cs" company="Kenneth Bell, Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A descriptor for a directory record.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#CatalogFile"/>
    public sealed class CatalogDirInfo : CommonCatalogFileInfo
    {
        /// <summary>
        /// Gets or sets bit flags about the folder. No bits are currently defined for folder records.
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Gets or sets the number of files and folders directly contained by this folder.
        /// </summary>
        public uint Valence { get; set; }

        /// <inheritdoc/>
        public override int Size => 52;

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            int read = base.ReadFrom(buffer, offset);

            this.Flags = EndianUtilities.ToUInt16BigEndian(buffer, offset + 2);
            this.Valence = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);

            return read + 6;
        }

        /// <inheritdoc/>
        public override void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}