// <copyright file="BTreeNodeDescriptor.cs" company="Kenneth Bell, Quamotion bv">
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
    /// The node descriptor contains basic information about the node as well as forward and backward links to other nodes.
    /// </summary>
    public sealed class BTreeNodeDescriptor : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the node number of the previous node of this type, or 0 if this is the first node.
        /// </summary>
        public uint BackwardLink { get; set; }

        /// <summary>
        /// Gets or sets the node number of the next node of this type, or 0 if this is the last node.
        /// </summary>
        public uint ForwardLink { get; set; }

        /// <summary>
        /// Gets or sets the level, or depth, of this node in the B-tree hierarchy.
        /// </summary>
        public byte Height { get; set; }

        /// <summary>
        /// Gets or sets the type of this node.
        /// </summary>
        public BTreeNodeKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the number of records contained in this node.
        /// </summary>
        public ushort NumRecords { get; set; }

        /// <summary>
        /// Gets or sets a value which must be treated as a reserved field.
        /// </summary>
        public ushort Reserved { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return 14; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.ForwardLink = EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.BackwardLink = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.Kind = (BTreeNodeKind)buffer[offset + 8];
            this.Height = buffer[offset + 9];
            this.NumRecords = EndianUtilities.ToUInt16BigEndian(buffer, offset + 10);
            this.Reserved = EndianUtilities.ToUInt16BigEndian(buffer, offset + 12);

            return 14;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}