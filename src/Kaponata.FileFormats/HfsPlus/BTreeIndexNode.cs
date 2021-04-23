// <copyright file="BTreeIndexNode.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Collections.Generic;

namespace DiscUtils.HfsPlus
{
    internal class BTreeIndexNode<TKey> : BTreeKeyedNode<TKey>
        where TKey : BTreeKey, new()
    {
        private BTreeIndexRecord<TKey>[] records;

        public BTreeIndexNode(BTree tree, BTreeNodeDescriptor descriptor)
            : base(tree, descriptor) {}

        /// <inheritdoc/>
        public override byte[] FindKey(TKey key)
        {
            int nextResult = this.records[0].Key.CompareTo(key);

            int idx = 0;
            while (idx < this.records.Length)
            {
                int thisResult = nextResult;

                if (idx + 1 < this.records.Length)
                {
                    nextResult = this.records[idx + 1].Key.CompareTo(key);
                }
                else
                {
                    nextResult = 1;
                }

                if (thisResult > 0)
                {
                    // This record's key is too big, so no chance further records
                    // will match.
                    return null;
                }
                if (nextResult > 0)
                {
                    // Next record's key is too big, so worth looking at children
                    BTreeKeyedNode<TKey> child = ((BTree<TKey>)this.Tree).GetKeyedNode(this.records[idx].ChildId);
                    return child.FindKey(key);
                }

                idx++;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void VisitRange(BTreeVisitor<TKey> visitor)
        {
            int nextResult = visitor(this.records[0].Key, null);

            int idx = 0;
            while (idx < this.records.Length)
            {
                int thisResult = nextResult;

                if (idx + 1 < this.records.Length)
                {
                    nextResult = visitor(this.records[idx + 1].Key, null);
                }
                else
                {
                    nextResult = 1;
                }

                if (thisResult > 0)
                {
                    // This record's key is too big, so no chance further records
                    // will match.
                    return;
                }
                if (nextResult >= 0)
                {
                    // Next record's key isn't too small, so worth looking at children
                    BTreeKeyedNode<TKey> child = ((BTree<TKey>)this.Tree).GetKeyedNode(this.records[idx].ChildId);
                    child.VisitRange(visitor);
                }

                idx++;
            }
        }

        /// <inheritdoc/>
        protected override IList<BTreeNodeRecord> ReadRecords(byte[] buffer, int offset)
        {
            int numRecords = this.Descriptor.NumRecords;
            int nodeSize = this.Tree.NodeSize;

            this.records = new BTreeIndexRecord<TKey>[numRecords];

            int start = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - 2);

            for (int i = 0; i < numRecords; ++i)
            {
                int end = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - (i + 2) * 2);

                this.records[i] = new BTreeIndexRecord<TKey>(end - start);
                this.records[i].ReadFrom(buffer, offset + start);

                start = end;
            }

            return this.records;
        }
    }
}