// <copyright file="PacketHeader.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Supports reading and writing <see cref="PacketHeader"/> values.
    /// </summary>
    public partial struct PacketHeader
    {
        /// <summary>
        /// Gets the binary size of a packet header.
        /// </summary>
        public const int BinarySize = 12;

        /// <summary>
        /// Reads a <see cref="PacketHeader"/> value from a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer from which to read the <see cref="PacketHeader"/> value.
        /// </param>
        /// <returns>
        /// A <see cref="PacketHeader"/> value.
        /// </returns>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/stream.c#L25"/>
        public static PacketHeader Read(Span<byte> buffer)
        {
            return new PacketHeader()
            {
                PacketTimeStamp = BinaryPrimitives.ReadUInt64BigEndian(buffer.Slice(0, 8)),
                PacketLength = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(8, 4)),
            };
        }
    }
}
