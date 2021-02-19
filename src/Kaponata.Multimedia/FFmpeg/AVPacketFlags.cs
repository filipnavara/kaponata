// <copyright file="AVPacketFlags.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeFFmpeg = FFmpeg.AutoGen.ffmpeg;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Contains metadata about a <see cref="AVPacket"/>.
    /// </summary>
    [Flags]
    public enum AVPacketFlags
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The packet is corrupt.
        /// </summary>
        Corrupt = NativeFFmpeg.AV_PKT_FLAG_CORRUPT,

        /// <summary>
        /// The packet has been discared.
        /// </summary>
        Discard = NativeFFmpeg.AV_PKT_FLAG_DISCARD,

        /// <summary>
        /// The packet contains a key frame.
        /// </summary>
        Key = NativeFFmpeg.AV_PKT_FLAG_KEY,
    }
}
