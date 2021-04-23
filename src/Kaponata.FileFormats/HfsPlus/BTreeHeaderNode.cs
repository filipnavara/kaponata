// <copyright file="BTreeHeaderNode.cs" company="Kenneth Bell, Quamotion bv">
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
    internal class BTreeHeaderNode : BTreeNode
    {
        public BTreeHeaderNode(BTree tree, BTreeNodeDescriptor descriptor)
            : base(tree, descriptor)
        {
        }

        public BTreeHeaderRecord HeaderRecord
        {
            get { return this.Records[0] as BTreeHeaderRecord; }
        }

        /// <inheritdoc/>
        protected override IList<BTreeNodeRecord> ReadRecords(byte[] buffer, int offset)
        {
            int totalRecords = this.Descriptor.NumRecords;
            int nodeSize = this.Tree.NodeSize;

            int headerRecordOffset = EndianUtilities.ToUInt16BigEndian(buffer, nodeSize - 2);
            int userDataRecordOffset = EndianUtilities.ToUInt16BigEndian(buffer, nodeSize - 4);
            int mapRecordOffset = EndianUtilities.ToUInt16BigEndian(buffer, nodeSize - 6);

            BTreeNodeRecord[] results = new BTreeNodeRecord[3];
            results[0] = new BTreeHeaderRecord();
            results[0].ReadFrom(buffer, offset + headerRecordOffset);

            results[1] = new BTreeGenericRecord(mapRecordOffset - userDataRecordOffset);
            results[1].ReadFrom(buffer, offset + userDataRecordOffset);

            results[2] = new BTreeGenericRecord(nodeSize - ((totalRecords * 2) + mapRecordOffset));
            results[2].ReadFrom(buffer, offset + mapRecordOffset);

            return results;
        }
    }
}