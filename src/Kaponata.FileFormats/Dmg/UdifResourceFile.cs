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

#nullable disable

using DiscUtils.Streams;
using System;

namespace DiscUtils.Dmg
{
    internal class UdifResourceFile : IByteArraySerializable
    {
        public UdifChecksum DataForkChecksum { get; set; }

        public ulong DataForkLength { get; set; }

        public ulong DataForkOffset { get; set; }

        public uint Flags { get; set; }

        public uint HeaderSize { get; set; }

        public uint ImageVariant { get; set; }

        public UdifChecksum MasterChecksum { get; set; }

        public ulong RsrcForkLength { get; set; }

        public ulong RsrcForkOffset { get; set; }

        public ulong RunningDataForkOffset { get; set; }

        public long SectorCount { get; set; }

        public uint SegmentCount { get; set; }

        public Guid SegmentGuid { get; set; }

        public uint SegmentNumber { get; set; }

        public uint Signature { get; set; }

        public uint Version { get; set; }

        public ulong XmlLength { get; set; }

        public ulong XmlOffset { get; set; }

        public bool SignatureValid
        {
            get { return this.Signature == 0x6B6F6C79; }
        }

        /// <inheritdoc/>
        public int Size
        {
            get { return 512; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Signature = EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.Version = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.HeaderSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.Flags = EndianUtilities.ToUInt32BigEndian(buffer, offset + 12);
            this.RunningDataForkOffset = EndianUtilities.ToUInt64BigEndian(buffer, offset + 16);
            this.DataForkOffset = EndianUtilities.ToUInt64BigEndian(buffer, offset + 24);
            this.DataForkLength = EndianUtilities.ToUInt64BigEndian(buffer, offset + 32);
            this.RsrcForkOffset = EndianUtilities.ToUInt64BigEndian(buffer, offset + 40);
            this.RsrcForkLength = EndianUtilities.ToUInt64BigEndian(buffer, offset + 48);
            this.SegmentNumber = EndianUtilities.ToUInt32BigEndian(buffer, offset + 56);
            this.SegmentCount = EndianUtilities.ToUInt32BigEndian(buffer, offset + 60);
            this.SegmentGuid = EndianUtilities.ToGuidBigEndian(buffer, offset + 64);

            this.DataForkChecksum = EndianUtilities.ToStruct<UdifChecksum>(buffer, offset + 80);
            this.XmlOffset = EndianUtilities.ToUInt64BigEndian(buffer, offset + 216);
            this.XmlLength = EndianUtilities.ToUInt64BigEndian(buffer, offset + 224);

            this.MasterChecksum = EndianUtilities.ToStruct<UdifChecksum>(buffer, offset + 352);
            this.ImageVariant = EndianUtilities.ToUInt32BigEndian(buffer, offset + 488);
            this.SectorCount = EndianUtilities.ToInt64BigEndian(buffer, offset + 492);

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}