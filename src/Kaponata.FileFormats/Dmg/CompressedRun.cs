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
using System;

namespace DiscUtils.Dmg
{
    internal class CompressedRun : IByteArraySerializable
    {
        public long CompLength { get; set; }

        public long CompOffset { get; set; }

        public long SectorCount { get; set; }

        public long SectorStart { get; set; }

        public RunType Type { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return 40; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Type = (RunType)EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.SectorStart = EndianUtilities.ToInt64BigEndian(buffer, offset + 8);
            this.SectorCount = EndianUtilities.ToInt64BigEndian(buffer, offset + 16);
            this.CompOffset = EndianUtilities.ToInt64BigEndian(buffer, offset + 24);
            this.CompLength = EndianUtilities.ToInt64BigEndian(buffer, offset + 32);

            return 40;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}