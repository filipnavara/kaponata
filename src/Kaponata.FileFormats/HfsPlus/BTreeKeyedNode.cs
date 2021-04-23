// <copyright file="BTreeKeyedNode.cs" company="Kenneth Bell, Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents a node in a keyed B-tree.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    /// <seealso gref="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#KeyedRecords"/>
    internal abstract class BTreeKeyedNode<TKey> : BTreeNode
        where TKey : BTreeKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeKeyedNode{TKey}"/> class.
        /// </summary>
        /// <param name="tree">
        /// The tree to which this node belongs.
        /// </param>
        /// <param name="descriptor">
        /// A descriptor which describes the key.
        /// </param>
        public BTreeKeyedNode(BTree tree, BTreeNodeDescriptor descriptor)
            : base(tree, descriptor)
        {
        }

        /// <summary>
        /// Finds a node with a specific key.
        /// </summary>
        /// <param name="key">
        /// The key of the node to find.
        /// </param>
        /// <returns>
        /// The requested node.
        /// </returns>
        public abstract byte[] FindKey(TKey key);

        /// <summary>
        /// Visits a range in this node.
        /// </summary>
        /// <param name="visitor">
        /// A visitor.
        /// </param>
        public abstract void VisitRange(BTreeVisitor<TKey> visitor);
    }
}