// <copyright file="AttributeKey.cs" company="Quamotion bv">
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
    /// Represents the keys used by the attribute file.
    /// </summary>
    public sealed class AttributeKey : BTreeKey
    {
        private ushort keyLength;
        private ushort pad;
        private uint startBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeKey"/> class.
        /// </summary>
        public AttributeKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeKey"/> class.
        /// </summary>
        /// <param name="nodeId">
        /// The node ID of the node to which the attribute applies.
        /// </param>
        /// <param name="name">
        /// The name of the attribute.
        /// </param>
        public AttributeKey(CatalogNodeId nodeId, string name)
        {
            this.FileId = nodeId;
            this.Name = name;
        }

        /// <summary>
        /// Gets the ID of the file to which the attribute applies.
        /// </summary>
        public CatalogNodeId FileId { get; private set; }

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override int Size => this.keyLength + 2;

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            this.keyLength = EndianUtilities.ToUInt16BigEndian(buffer, offset + 0);
            this.pad = EndianUtilities.ToUInt16BigEndian(buffer, offset + 2);
            this.FileId = new CatalogNodeId(EndianUtilities.ToUInt32BigEndian(buffer, offset + 4));
            this.startBlock = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.Name = HfsPlusUtilities.ReadUniStr255(buffer, offset + 12);

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
            var attributeKey = other as AttributeKey;

            if (attributeKey == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (this.FileId != attributeKey.FileId)
            {
                return this.FileId < attributeKey.FileId ? -1 : 1;
            }

            return HfsPlusUtilities.FastUnicodeCompare(this.Name, attributeKey.Name);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as AttributeKey;

            if (other == null)
            {
                return false;
            }

            return this.CompareTo(other) == 0;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.FileId);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Name + " (" + this.FileId + ")";
        }
    }
}