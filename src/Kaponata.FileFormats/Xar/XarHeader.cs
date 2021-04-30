//-----------------------------------------------------------------------
// <copyright file="XarHeader.cs" company="Quamotion bv">
//     Copyright (c) 2016 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using DiscUtils.Streams;
using System;
using System.Buffers.Binary;

namespace Kaponata.FileFormats.Xar
{
    /// <summary>
    /// Represents the header of a <see cref="XarFile"/>.
    /// </summary>
    public struct XarHeader : IByteArraySerializable
    {
        /// <summary>
        /// The magic for a XAR file. Represents <c>xar!</c> in ASCII.
        /// </summary>
        public const uint Magic = 0x78617221;

        /// <summary>
        /// The current version of the Xar header.
        /// </summary>
        public const uint CurrentVersion = 1;

        /// <summary>
        /// The signature of the header. Should equal <see cref="Magic"/>.
        /// </summary>
        public uint Signature;

        /// <summary>
        /// The size of the header.
        /// </summary>
        public ushort Size;

        /// <summary>
        /// The version of the header format. Should equal <see cref="CurrentVersion"/>.
        /// </summary>
        public ushort Version;

        /// <summary>
        /// The compressed length of the table of contents.
        /// </summary>
        public ulong TocLengthCompressed;

        /// <summary>
        /// The uncompressed length of the table of contents.
        /// </summary>
        public ulong TocLengthUncompressed;

        /// <summary>
        /// The algorithm used to calculate checksums.
        /// </summary>
        public XarChecksum ChecksumAlgorithm;

        /// <inheritdoc/>
        int IByteArraySerializable.Size => 28;

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Signature = BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(offset, 4));
            this.Size = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(offset + 4, 2));
            this.Version = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(offset + 6, 2));
            this.TocLengthCompressed = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(offset + 8, 8));
            this.TocLengthUncompressed = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(offset + 16, 8));
            this.ChecksumAlgorithm = (XarChecksum)BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(offset + 24, 4));

            return this.Size;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
