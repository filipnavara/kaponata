// <copyright file="BTreeIndexRecord.cs" company="Kenneth Bell, Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents a record in an index node, known as a pointer record.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key of the index node.
    /// </typeparam>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#IndexNodes"/>
    internal sealed class BTreeIndexRecord<TKey> : BTreeNodeRecord
        where TKey : BTreeKey, new()
    {
        private readonly int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeIndexRecord{TKey}"/> class.
        /// </summary>
        /// <param name="size">
        /// The size of the record.
        /// </param>
        public BTreeIndexRecord(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// Gets the node number of this index record.
        /// </summary>
        public uint ChildId { get; private set; }

        /// <summary>
        /// Gets the key of this index record.
        /// </summary>
        public TKey Key { get; private set; }

        /// <inheritdoc/>
        public override int Size
        {
            get { return this.size; }
        }

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            this.Key = new TKey();
            int keySize = this.Key.ReadFrom(buffer, offset);

            if ((keySize & 1) != 0)
            {
                ++keySize;
            }

            this.ChildId = EndianUtilities.ToUInt32BigEndian(buffer, offset + keySize);

            return this.size;
        }

        /// <inheritdoc/>
        public override void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Key + ":" + this.ChildId;
        }
    }
}