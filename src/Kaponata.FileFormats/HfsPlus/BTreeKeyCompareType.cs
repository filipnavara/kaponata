// <copyright file="BTreeKeyCompareType.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Defines the ordering of the keys in the catalog B-tree of a HFSX volume.
    /// </summary>
    public enum BTreeKeyCompareType : byte
    {
        /// <summary>
        /// Case folding (case-insensitive).
        /// </summary>
        CaseFolding = 0xCF,

        /// <summary>
        /// Binary compare (case-sensitive).
        /// </summary>
        BinaryCompare = 0xBC,
    }
}
