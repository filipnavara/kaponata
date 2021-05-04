// <copyright file="BTreeLeafRecord.cs" company="Kenneth Bell, Quamotion bv">
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

using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A record of a leaf node.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key of the node.
    /// </typeparam>
    public sealed class BTreeLeafRecord<TKey> : BTreeNodeRecord
        where TKey : BTreeKey, new()
    {
        private readonly int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeLeafRecord{TKey}"/> class.
        /// </summary>
        /// <param name="size">
        /// The size of the record.
        /// </param>
        public BTreeLeafRecord(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// Gets the data associated with the key of this node.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the key of this node.
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

            this.Data = new byte[this.size - keySize];
            Array.Copy(buffer, offset + keySize, this.Data, 0, this.Data.Length);

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
            return $"{this.Key}: {this.Data.Length} bytes";
        }
    }
}