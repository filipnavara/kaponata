﻿//-----------------------------------------------------------------------
// <copyright file="XarChecksum.cs" company="Quamotion bv">
//     Copyright (c) 2016 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Kaponata.FileFormats.Xar
{
    /// <summary>
    /// Represents an algorithm used to calculate the checksum of data inside a
    /// Xar archive.
    /// </summary>
    public enum XarChecksum : uint
    {
        /// <summary>
        /// No checksums have been calculated.
        /// </summary>
        None = 0,

        /// <summary>
        /// The SHA1 algorithm has been used to calculate the checksums.
        /// </summary>
        Sha1 = 1,

        /// <summary>
        /// The MD5 algorithm has been used to calculate the checksums.
        /// </summary>
        MD5 = 2,

        /// <summary>
        /// Another algorithm has been used to calculate the checksums.
        /// </summary>
        Other = 3,
    }
}
