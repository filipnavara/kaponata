// <copyright file="CatalogThread.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Text;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// The catalog thread record is used in the catalog B-tree file to link a <see cref="CatalogNodeId"/> to the file or folder record using that CNID.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#CatalogThreadRecord"/>
    public sealed class CatalogThread : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the name of the file or folder referenced by this thread record.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CNID of the parent of the file or folder referenced by this thread record.
        /// </summary>
        public CatalogNodeId ParentId { get; set; }

        /// <summary>
        /// Gets or sets the The catalog data record type.
        /// </summary>
        public CatalogRecordType RecordType { get; set; }

        /// <inheritdoc/>
        public int Size => 10 + Encoding.BigEndianUnicode.GetByteCount(this.Name);

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.RecordType = (CatalogRecordType)EndianUtilities.ToInt16BigEndian(buffer, offset + 0);
            this.ParentId = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.Name = HfsPlusUtilities.ReadUniStr255(buffer, offset + 8);

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}