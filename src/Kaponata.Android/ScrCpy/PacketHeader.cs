// <copyright file="PacketHeader.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Represents the packet header of the scrcpy video stream.
    /// </summary>
    public partial struct PacketHeader
    {
        /// <summary>
        /// The packet timestamp expressed in microseconds.
        /// The first packet has the <see cref="ulong.MaxValue"/> timestamp.
        /// </summary>
        public ulong PacketTimeStamp;

        /// <summary>
        /// The packet Length.
        /// </summary>
        public uint PacketLength;
    }
}
