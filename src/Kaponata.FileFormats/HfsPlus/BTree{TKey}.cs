// <copyright file="BTree{TKey}.cs" company="Kenneth Bell, Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents an B-tree.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key used by this B-tree.
    /// </typeparam>
    internal sealed class BTree<TKey> : BTree
        where TKey : BTreeKey, new()
    {
        private readonly IBuffer data;
        private readonly BTreeHeaderRecord header;
        private readonly BTreeKeyedNode<TKey> rootNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BTree{TKey}"/> class.
        /// </summary>
        /// <param name="data">
        /// The raw data which represents the B-tree.
        /// </param>
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

        /// <inheritdoc/>
        internal override int NodeSize
        {
            get { return this.header.NodeSize; }
        }

        /// <summary>
        /// Finds a node in this B-tree.
        /// </summary>
        /// <param name="key">
        /// The key of the node to find.
        /// </param>
        /// <returns>
        /// If found, the requested node. Otherwise, <see langword="null"/>.
        /// </returns>
        public byte[] Find(TKey key)
        {
            return this.rootNode == null ? null : this.rootNode.FindKey(key);
        }

        /// <summary>
        /// Visits a range.
        /// </summary>
        /// <param name="visitor">
        /// The node visitor.
        /// </param>
        public void VisitRange(BTreeVisitor<TKey> visitor)
        {
            this.rootNode.VisitRange(visitor);
        }

        /// <summary>
        /// Gets a keyed node.
        /// </summary>
        /// <param name="nodeId">
        /// The id of the node.
        /// </param>
        /// <returns>
        /// The requested node.
        /// </returns>
        internal BTreeKeyedNode<TKey> GetKeyedNode(uint nodeId)
        {
            byte[] nodeData = StreamUtilities.ReadExact(this.data, (int)nodeId * this.header.NodeSize, this.header.NodeSize);

            BTreeKeyedNode<TKey> node = BTreeNode.ReadNode<TKey>(this, nodeData, 0) as BTreeKeyedNode<TKey>;
            node.ReadFrom(nodeData, 0);
            return node;
        }
    }
}