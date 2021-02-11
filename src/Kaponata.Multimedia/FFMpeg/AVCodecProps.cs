// <copyright file="AVCodecProps.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Defines additional properties of a codec.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/2.5/group__lavc__core.html"/>
    public enum AVCodecProps
    {
        /// <summary>
        /// No properties are defined for this codec.
        /// </summary>
        None = 0,

        /// <summary>
        /// The subtitle codec is bitmap based.
        /// </summary>
        BITMAP_SUB = ffmpeg.AV_CODEC_PROP_BITMAP_SUB,

        /// <summary>
        /// The codec uses only intra compression.
        /// </summary>
        INTRA_ONLY = ffmpeg.AV_CODEC_PROP_INTRA_ONLY,

        /// <summary>
        /// The codec supports lossless compression.
        /// </summary>
        LOSSLESS = ffmpeg.AV_CODEC_PROP_LOSSLESS,

        /// <summary>
        /// The codec supports lossy compression.
        /// </summary>
        LOSSY = ffmpeg.AV_CODEC_PROP_LOSSY,

        /// <summary>
        /// The codec supports frame reordering.
        /// </summary>
        REORDER = ffmpeg.AV_CODEC_PROP_REORDER,

        /// <summary>
        /// The subtitle codec is text based.
        /// </summary>
        TEXT_SUB = ffmpeg.AV_CODEC_PROP_TEXT_SUB,
    }
}
