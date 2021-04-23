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

namespace DiscUtils.HfsPlus
{
    internal sealed class BTree<TKey> : BTree
        where TKey : BTreeKey, new()
    {
        private readonly IBuffer data;
        private readonly BTreeHeaderRecord header;
        private readonly BTreeKeyedNode<TKey> rootNode;

        public BTree(IBuffer data)
        {
            this.data = data;

            byte[] headerInfo = StreamUtilities.ReadExact(this.data, 0, 114);

            this.header = new BTreeHeaderRecord();
            this.header.ReadFrom(headerInfo, 14);

            byte[] node0data = StreamUtilities.ReadExact(this.data, 0, this.header.NodeSize);

            BTreeHeaderNode node0 = BTreeNode.ReadNode(this, node0data, 0) as BTreeHeaderNode;
            node0.ReadFrom(node0data, 0);

            if (node0.HeaderRecord.RootNode != 0)
            {
                this.rootNode = this.GetKeyedNode(node0.HeaderRecord.RootNode);
            }
        }

        internal override int NodeSize
        {
            get { return this.header.NodeSize; }
        }

        public byte[] Find(TKey key)
        {
            return this.rootNode == null ? null : this.rootNode.FindKey(key);
        }

        public void VisitRange(BTreeVisitor<TKey> visitor)
        {
            this.rootNode.VisitRange(visitor);
        }

        internal BTreeKeyedNode<TKey> GetKeyedNode(uint nodeId)
        {
            byte[] nodeData = StreamUtilities.ReadExact(this.data, (int)nodeId * this.header.NodeSize, this.header.NodeSize);

            BTreeKeyedNode<TKey> node = BTreeNode.ReadNode<TKey>(this, nodeData, 0) as BTreeKeyedNode<TKey>;
            node.ReadFrom(nodeData, 0);
            return node;
        }
    }
}