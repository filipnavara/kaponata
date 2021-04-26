// <copyright file="BTreeHeaderRecord.cs" company="Kenneth Bell, Quamotion bv">
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
    /// The B-tree header record contains general information about the B-tree such as its size, maximum key length, and the location of the first and last leaf nodes.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#BTrees"/>
    public class BTreeHeaderRecord : BTreeNodeRecord
    {
        /// <summary>
        /// Gets or sets a set of bits used to describe various attributes of the B-tree.
        /// </summary>
        public BTreeAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets a value which is ignored for HFS Plus B-trees.
        /// </summary>
        public uint ClumpSize { get; set; }

        /// <summary>
        /// Gets or sets the node number of the first leaf node. This may be zero if there are no leaf nodes.
        /// </summary>
        public uint FirstLeafNode { get; set; }

        /// <summary>
        /// Gets or sets the number of unused nodes in the B-tree.
        /// </summary>
        public uint FreeNodes { get; set; }

        /// <summary>
        /// Gets or sets, for HFSX volumes, a value which defines the ordering of the keys (whether the volume is case-sensitive or case-insensitive).
        /// </summary>
        public BTreeKeyCompareType KeyCompareType { get; set; }

        /// <summary>
        /// Gets or sets the node number of the last leaf node. This may be zero if there are no leaf nodes.
        /// </summary>
        public uint LastLeafNode { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of a key in an index or leaf node.
        /// </summary>
        public ushort MaxKeyLength { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of a node. This is a power of two, from 512 through 32,768, inclusive.
        /// </summary>
        public ushort NodeSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of records contained in all of the leaf nodes.
        /// </summary>
        public uint NumLeafRecords { get; set; }

        /// <summary>
        /// Gets or sets the value of the first reserved field.
        /// </summary>
        public ushort Res1 { get; set; }

        /// <summary>
        /// Gets or sets he tnode number of the root node, the index node that acts as the root of the B-tree.
        /// </summary>
        public uint RootNode { get; set; }

        /// <summary>
        /// Gets or sets the total number of nodes (be they free or used) in the B-tree.
        /// </summary>
        public uint TotalNodes { get; set; }

        /// <summary>
        /// Gets or sets the current depth of the B-tree. Always equal to the height field of the root node.
        /// </summary>
        public ushort TreeDepth { get; set; }

        /// <summary>
        /// Gets or sets a value which determines the tree type.
        /// </summary>
        public BTreeType TreeType { get; set; }

        /// <inheritdoc/>
        public override int Size
        {
            get { return 104; }
        }

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            this.TreeDepth = EndianUtilities.ToUInt16BigEndian(buffer, offset + 0);
            this.RootNode = EndianUtilities.ToUInt32BigEndian(buffer, offset + 2);
            this.NumLeafRecords = EndianUtilities.ToUInt32BigEndian(buffer, offset + 6);
            this.FirstLeafNode = EndianUtilities.ToUInt32BigEndian(buffer, offset + 10);
            this.LastLeafNode = EndianUtilities.ToUInt32BigEndian(buffer, offset + 14);
            this.NodeSize = EndianUtilities.ToUInt16BigEndian(buffer, offset + 18);
            this.MaxKeyLength = EndianUtilities.ToUInt16BigEndian(buffer, offset + 20);
            this.TotalNodes = EndianUtilities.ToUInt32BigEndian(buffer, offset + 22);
            this.FreeNodes = EndianUtilities.ToUInt32BigEndian(buffer, offset + 26);
            this.Res1 = EndianUtilities.ToUInt16BigEndian(buffer, offset + 30);
            this.ClumpSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 32);
            this.TreeType = (BTreeType)buffer[offset + 36];
            this.KeyCompareType = (BTreeKeyCompareType)buffer[offset + 37];
            this.Attributes = (BTreeAttributes)EndianUtilities.ToUInt32BigEndian(buffer, offset + 38);

            // 16 bytes of reserved data
            return 104;
        }

        /// <inheritdoc/>
        public override void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}