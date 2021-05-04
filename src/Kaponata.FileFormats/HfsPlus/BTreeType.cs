// <copyright file="BTreeType.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Specifies the type of the B-tree.
    /// </summary>
    public enum BTreeType : byte
    {
        /// <summary>
        /// The tree is a catalog, extents or attributes B-tree.
        /// </summary>
        HFSBTreeType = 0,

        /// <summary>
        /// The tree is a hot file B-tree.
        /// </summary>
        UserBTreeType = 128,
    }
}
