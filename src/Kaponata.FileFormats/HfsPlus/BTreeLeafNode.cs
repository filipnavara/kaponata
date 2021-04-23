// <copyright file="BTreeLeafNode.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Collections.Generic;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents a leaf node (the bottom level) of a B-tree. Leaf nodes contain data records instead of pointer records.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key of the node.
    /// </typeparam>
    internal sealed class BTreeLeafNode<TKey> : BTreeKeyedNode<TKey>
        where TKey : BTreeKey, new()
    {
        private BTreeLeafRecord<TKey>[] records;

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeLeafNode{TKey}"/> class.
        /// </summary>
        /// <param name="tree">
        /// The tree to which the leaf node belongs.
        /// </param>
        /// <param name="descriptor">
        /// A descriptor which describes the leaf node.
        /// </param>
        public BTreeLeafNode(BTree tree, BTreeNodeDescriptor descriptor)
            : base(tree, descriptor)
        {
        }

        /// <inheritdoc/>
        public override byte[] FindKey(TKey key)
        {
            int idx = 0;
            while (idx < this.records.Length)
            {
                int compResult = key.CompareTo(this.records[idx].Key);
                if (compResult == 0)
                {
                    return this.records[idx].Data;
                }

                if (compResult < 0)
                {
                    return null;
                }

                ++idx;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void VisitRange(BTreeVisitor<TKey> visitor)
        {
            int idx = 0;
            while (idx < this.records.Length && visitor(this.records[idx].Key, this.records[idx].Data) <= 0)
            {
                idx++;
            }
        }

        /// <inheritdoc/>
        protected override IList<BTreeNodeRecord> ReadRecords(byte[] buffer, int offset)
        {
            int numRecords = this.Descriptor.NumRecords;
            int nodeSize = this.Tree.NodeSize;

            this.records = new BTreeLeafRecord<TKey>[numRecords];

            int start = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - 2);

            for (int i = 0; i < numRecords; ++i)
            {
                int end = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - ((i + 2) * 2));

                this.records[i] = new BTreeLeafRecord<TKey>(end - start);
                this.records[i].ReadFrom(buffer, offset + start);

                start = end;
            }

            return this.records;
        }
    }
}