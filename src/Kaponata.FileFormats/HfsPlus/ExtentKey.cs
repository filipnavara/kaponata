// <copyright file="ExtentKey.cs" company="Kenneth Bell, Quamotion bv">
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
    /// The key for an extent node.
    /// </summary>
    internal sealed class ExtentKey : BTreeKey, IComparable<ExtentKey>
    {
        private byte forkType; // 0 is data, 0xff is rsrc
        private ushort keyLength;
        private uint startBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtentKey"/> class.
        /// </summary>
        public ExtentKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtentKey"/> class.
        /// </summary>
        /// <param name="cnid">
        /// The <see cref="CatalogNodeId"/> of the file to which the extent belongs.
        /// </param>
        /// <param name="startBlock">
        /// The first block of the extent.
        /// </param>
        /// <param name="resource_fork">
        /// A value indicating whether the extent is a resource fork.
        /// </param>
        public ExtentKey(CatalogNodeId cnid, uint startBlock, bool resource_fork = false)
        {
            this.keyLength = 10;
            this.NodeId = cnid;
            this.startBlock = startBlock;
            this.forkType = (byte)(resource_fork ? 0xff : 0x00);
        }

        /// <summary>
        /// Gets <see cref="CatalogNodeId"/> of the file to which the extent belongs.
        /// </summary>
        public CatalogNodeId NodeId { get; private set; }

        /// <inheritdoc/>
        public override int Size
        {
            get { return 12; }
        }

        /// <inheritdoc/>
        public int CompareTo(ExtentKey other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            // Sort by file id, fork type, then starting block
            if (this.NodeId != other.NodeId)
            {
                return this.NodeId < other.NodeId ? -1 : 1;
            }

            if (this.forkType != other.forkType)
            {
                return this.forkType < other.forkType ? -1 : 1;
            }

            if (this.startBlock != other.startBlock)
            {
                return this.startBlock < other.startBlock ? -1 : 1;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            this.keyLength = EndianUtilities.ToUInt16BigEndian(buffer, offset + 0);
            this.forkType = buffer[offset + 2];
            this.NodeId = new CatalogNodeId(EndianUtilities.ToUInt32BigEndian(buffer, offset + 4));
            this.startBlock = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
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
            return this.CompareTo(other as ExtentKey);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "ExtentKey (" + this.NodeId + " - " + this.startBlock + ")";
        }
    }
}