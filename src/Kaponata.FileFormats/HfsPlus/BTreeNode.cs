// <copyright file="BTreeNode.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Collections.Generic;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents a node in a B-tree.
    /// </summary>
    public abstract class BTreeNode : IByteArraySerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeNode"/> class.
        /// </summary>
        /// <param name="tree">
        /// The B-tree to which the node belongs.
        /// </param>
        /// <param name="descriptor">
        /// A descriptor for the node.
        /// </param>
        public BTreeNode(BTree tree, BTreeNodeDescriptor descriptor)
        {
            this.Tree = tree;
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets the records embedded in this node.
        /// </summary>
        public IList<BTreeNodeRecord> Records { get; private set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return this.Tree.NodeSize; }
        }

        /// <summary>
        /// Gets the <see cref="BTree"/> to which this node belongs.
        /// </summary>
        protected BTree Tree { get; }

        /// <summary>
        /// Gets the descriptor for this node.
        /// </summary>
        protected BTreeNodeDescriptor Descriptor { get; }

        /// <summary>
        /// Reads a <see cref="BTreeNode"/> from its binary representation.
        /// </summary>
        /// <param name="tree">
        /// The tree to which the node belongs.
        /// </param>
        /// <param name="buffer">
        /// The buffer which contains the node data.
        /// </param>
        /// <param name="offset">
        /// The offset at which to start reading the node data.
        /// </param>
        /// <returns>
        /// A <see cref="BTreeNode"/> which represents the data.
        /// </returns>
        public static BTreeNode ReadNode(BTree tree, byte[] buffer, int offset)
        {
            BTreeNodeDescriptor descriptor =
                EndianUtilities.ToStruct<BTreeNodeDescriptor>(buffer, offset);

            switch (descriptor.Kind)
            {
                case BTreeNodeKind.HeaderNode:
                    return new BTreeHeaderNode(tree, descriptor);
                case BTreeNodeKind.IndexNode:
                case BTreeNodeKind.LeafNode:
                    throw new NotImplementedException("Attempt to read index/leaf node without key and data types");
                default:
                    throw new NotImplementedException("Unrecognized BTree node kind: " + descriptor.Kind);
            }
        }

        /// <summary>
        /// Reads a <see cref="BTreeNode"/> from its binary representation.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the key of th enode.
        /// </typeparam>
        /// <param name="tree">
        /// The tree to which the node belongs.
        /// </param>
        /// <param name="buffer">
        /// The buffer which contains the node data.
        /// </param>
        /// <param name="offset">
        /// The offset at which to start reading the node data.
        /// </param>
        /// <returns>
        /// A <see cref="BTreeNode"/> which represents the data.
        /// </returns>
        public static BTreeNode ReadNode<TKey>(BTree tree, byte[] buffer, int offset)
            where TKey : BTreeKey, new()
        {
            BTreeNodeDescriptor descriptor =
                EndianUtilities.ToStruct<BTreeNodeDescriptor>(buffer, offset);

            switch (descriptor.Kind)
            {
                case BTreeNodeKind.HeaderNode:
                    return new BTreeHeaderNode(tree, descriptor);
                case BTreeNodeKind.LeafNode:
                    return new BTreeLeafNode<TKey>(tree, descriptor);
                case BTreeNodeKind.IndexNode:
                    return new BTreeIndexNode<TKey>(tree, descriptor);
                default:
                    throw new NotImplementedException("Unrecognized BTree node kind: " + descriptor.Kind);
            }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Records = this.ReadRecords(buffer, offset);

            return 0;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the records embedded in this node.
        /// </summary>
        /// <param name="buffer">
        /// The buffer which contains the record data.
        /// </param>
        /// <param name="offset">
        /// The offset at which to start reading the record data.
        /// </param>
        /// <returns>
        /// A list of records embedded in this node.
        /// </returns>
        protected virtual IList<BTreeNodeRecord> ReadRecords(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}