// <copyright file="SizedDeflateStream.cs" company="Quamotion bv">
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

using System;
using System.IO;
using System.IO.Compression;

namespace DiscUtils.Compression
{
    /// <summary>
    /// A <see cref="DeflateStream"/> which is aware of its size.
    /// </summary>
    internal class SizedDeflateStream : DeflateStream
    {
        private readonly int length;
        private int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="SizedDeflateStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The stream to compress or decompress.
        /// </param>
        /// <param name="mode">
        /// One of the enumeration values that indicates whether to compress or decompress
        /// the stream.
        /// </param>
        /// <param name="leaveOpen">
        /// <see langword="true"/> to leave the stream open after disposing the <see cref="SizedDeflateStream"/>;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="length">
        /// The decompressed data size.
        /// </param>
        public SizedDeflateStream(Stream stream, CompressionMode mode, bool leaveOpen, int length)
            : base(stream, mode, leaveOpen)
        {
            this.length = length;
        }

        /// <inheritdoc/>
        public override long Length
        {
            get { return this.length; }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (value != this.Position)
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] array, int offset, int count)
        {
            int read = base.Read(array, offset, count);
            this.position += read;
            return read;
        }
    }
}