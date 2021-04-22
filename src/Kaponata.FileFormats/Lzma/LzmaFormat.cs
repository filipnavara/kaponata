// <copyright file="LzmaFormat.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.FileFormats.Lzma
{
    /// <summary>
    /// Specifies which of the formats supported by liblzma are accepted.
    /// </summary>
    public enum LzmaFormat
    {
        /// <summary>
        /// Decode .xz Streams and .lzma files with autodetection.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Only decode .xz streams.
        /// </summary>
        Xz = 1,

        /// <summary>
        /// Only decode .lzma files.
        /// </summary>
        Lzma = 2,
    }
}
