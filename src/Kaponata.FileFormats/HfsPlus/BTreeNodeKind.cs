// <copyright file="BTreeNodeKind.cs" company="Kenneth Bell, Quamotion bv">
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
    /// Describes the type of a node, which indicates what kinds of records it contains and, therefore, its purpose in the B-tree hierarchy.
    /// </summary>
    public enum BTreeNodeKind : sbyte
    {
        /// <summary>
        /// The node is a leaf node. Leaf nodes contain binary data.
        /// </summary>
        LeafNode = -1,

        /// <summary>
        /// The node is an index node.
        /// </summary>
        IndexNode = 0,

        /// <summary>
        /// The node is a header node.
        /// </summary>
        HeaderNode = 1,

        /// <summary>
        /// The node is a map node.
        /// </summary>
        MapNode = 2,
    }
}