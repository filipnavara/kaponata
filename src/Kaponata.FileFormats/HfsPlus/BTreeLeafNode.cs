//
// Copyright (c) 2008-2011, Kenneth Bell
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
//

using DiscUtils.Streams;
using System.Collections.Generic;

namespace DiscUtils.HfsPlus
{
    internal sealed class BTreeLeafNode<TKey> : BTreeKeyedNode<TKey>
        where TKey : BTreeKey, new()
    {
        private BTreeLeafRecord<TKey>[] records;

        public BTreeLeafNode(BTree tree, BTreeNodeDescriptor descriptor)
            : base(tree, descriptor) {}

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

        public override void VisitRange(BTreeVisitor<TKey> visitor)
        {
            int idx = 0;
            while (idx < this.records.Length && visitor(this.records[idx].Key, this.records[idx].Data) <= 0)
            {
                idx++;
            }
        }

        protected override IList<BTreeNodeRecord> ReadRecords(byte[] buffer, int offset)
        {
            int numRecords = this.Descriptor.NumRecords;
            int nodeSize = this.Tree.NodeSize;

            this.records = new BTreeLeafRecord<TKey>[numRecords];

            int start = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - 2);

            for (int i = 0; i < numRecords; ++i)
            {
                int end = EndianUtilities.ToUInt16BigEndian(buffer, offset + nodeSize - (i + 2) * 2);

                this.records[i] = new BTreeLeafRecord<TKey>(end - start);
                this.records[i].ReadFrom(buffer, offset + start);

                start = end;
            }

            return this.records;
        }
    }
}