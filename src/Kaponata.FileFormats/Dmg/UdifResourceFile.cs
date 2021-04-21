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
    /// <summary>
    /// The <see cref="UdifResourceFile"/> represents the start of the 512-byte trailer
    /// of a DMG file. Because it uses <c>koly</c> as its magic, it is also known as the
    /// <c>koly</c> block.
    /// </summary>
    /// <seealso href="http://newosxbook.com/DMG.html"/>
    internal class UdifResourceFile : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets a <see cref="UdifChecksum"/> which represents the checksum
        /// for the data fork.
        /// </summary>
        public UdifChecksum DataForkChecksum { get; set; }

        /// <summary>
        /// Gets or sets the size of the data fork. The data fork usually runs
        /// through <see cref="XmlOffset"/>.
        /// </summary>
        public ulong DataForkLength { get; set; }

        /// <summary>
        /// Gets or sets the offset of the data fork. This is usally 0, at the
        /// beginning of the file.
        /// </summary>
        public ulong DataForkOffset { get; set; }

        /// <summary>
        /// Gets or sets optional flags.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the size of the header. Should always be 512.
        /// </summary>
        public uint HeaderSize { get; set; }

        /// <summary>
        /// Gets or sets a value which identifies the image variant. This is usually 1.
        /// </summary>
        public uint ImageVariant { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="UdifChecksum"/> value which represents the checksum
        /// of the master block.
        /// </summary>
        public UdifChecksum MasterChecksum { get; set; }

        /// <summary>
        /// Gets or sets the length of the resource fork, when present.
        /// </summary>
        public ulong RsrcForkLength { get; set; }

        /// <summary>
        /// Gets or sets the offset of the resource fork, when present.
        /// </summary>
        public ulong RsrcForkOffset { get; set; }

        /// <summary>
        /// Gets or sets the offset of the running data fork.
        /// </summary>
        public ulong RunningDataForkOffset { get; set; }

        /// <summary>
        /// Gets or sets the size of the data in the DMG file, expressed in sectors.
        /// </summary>
        public long SectorCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of segments. This is usually 1.
        /// </summary>
        public uint SegmentCount { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Guid"/> which identifies the current segment.
        /// Present when <see cref="SegmentNumber"/> is not zero.
        /// </summary>
        public Guid SegmentGuid { get; set; }

        /// <summary>
        /// Gets or sets the number of this segment. This is usually 1.
        /// </summary>
        public uint SegmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the signature (or magic) of the <see cref="UdifResourceFile"/>.
        /// Should always be <c>koly</c>.
        /// </summary>
        public uint Signature { get; set; }

        /// <summary>
        /// Gets or sets the version of the header. The current version is 4.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the length of the property list in the DMG file.
        /// </summary>
        public ulong XmlLength { get; set; }

        /// <summary>
        /// Gets or sets the offset of the property list in the DMG file.
        /// </summary>
        public ulong XmlOffset { get; set; }

        /// <summary>
        /// Gets a value indicating whether the checksum is valid.
        /// </summary>
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