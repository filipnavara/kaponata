// <copyright file="AVFormatContextFlags.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Flags that specify the state of a <see cref="AVFormatContext"/>.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/0.6/avformat_8h.html"/>
    [Flags]
    public enum AVFormatContextFlags : uint
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Signal that no header is present (streams are added dynamically)
        /// </summary>
        NoHeader = ffmpeg.AVFMTCTX_NOHEADER,
    }
}
