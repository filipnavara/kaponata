// <copyright file="AVFormatContextEventFlags.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Event flags for the <see cref="AVFormatContext"/> class.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/2.4/avformat_8h.html"/>
    public enum AVFormatContextEventFlags : uint
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The call resulted in updated metadata.
        /// </summary>
        MetadataUpdated = ffmpeg.AVFMT_EVENT_FLAG_METADATA_UPDATED,
    }
}
