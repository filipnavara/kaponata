// <copyright file="CatalogKey.cs" company="Kenneth Bell, Quamotion bv">
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
    /// For a given file, folder, or thread record, the catalog file key consists of the parent folder's <see cref="CatalogNodeId"/> and the name of the file or folder.
    /// </summary>
    public sealed class CatalogKey : BTreeKey, IComparable<CatalogKey>
    {
        private ushort keyLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogKey"/> class.
        /// </summary>
        public CatalogKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogKey"/> class.
        /// </summary>
        /// <param name="nodeId">
        /// The parent record's <see cref="CatalogNodeId"/>.
        /// </param>
        /// <param name="name">
        /// The name of the node.
        /// </param>
        public CatalogKey(CatalogNodeId nodeId, string name)
        {
            this.NodeId = nodeId;
            this.Name = name;
            this.keyLength = (ushort)(2 /* keyLength */ + 4 /*nodeId */ + Encoding.BigEndianUnicode.GetByteCount(this.Name));
        }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <remarks>
        /// For file or folder records, this is the name of the file or folder inside the parentID folder.
        /// For thread records, this is the empty string.
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parent record's <see cref="CatalogNodeId"/>.
        /// </summary>
        /// <remarks>
        /// For file and folder records, this is the folder containing the file or folder represented by the record.
        /// For thread records, this is the CNID of the file or folder itself.
        /// </remarks>
        public CatalogNodeId NodeId { get; private set; }

        /// <inheritdoc/>
        public override int Size => this.keyLength + 2;

        /// <inheritdoc/>
        public int CompareTo(CatalogKey other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (this.NodeId != other.NodeId)
            {
                return this.NodeId < other.NodeId ? -1 : 1;
            }

            return HfsPlusUtilities.FastUnicodeCompare(this.Name, other.Name);
        }

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            this.keyLength = EndianUtilities.ToUInt16BigEndian(buffer, offset + 0);
            this.NodeId = new CatalogNodeId(EndianUtilities.ToUInt32BigEndian(buffer, offset + 2));
            this.Name = HfsPlusUtilities.ReadUniStr255(buffer, offset + 6);

            return this.keyLength + 2;
        }

        /// <inheritdoc/>
        public override void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int CompareTo(BTreeKey other)
        {
            return this.CompareTo(other as CatalogKey);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Name + " (" + this.NodeId + ")";
        }
    }
}