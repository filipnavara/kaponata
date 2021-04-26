// <copyright file="BTreeAttributes.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A set of bits used to describe various attributes of the B-tree.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#BTrees"/>
    [Flags]
    public enum BTreeAttributes : uint
    {
        /// <summary>
        /// This bit indicates that the B-tree was not closed properly and should be checked for consistency.
        /// </summary>
        BadClose = 1,

        /// <summary>
        /// If this bit is set, the keyLength field of the keys in index and leaf nodes is <see cref="ushort"/>;
        /// otherwise, it is a <see cref="byte"/>. This bit must be set for all HFS Plus B-trees.
        /// </summary>
        BigKeys = 2,

        /// <summary>
        /// If this bit is set, the keys in index nodes occupy the number of bytes indicated by their keyLength field;
        /// otherwise, the keys in index nodes always occupy maxKeyLength bytes. This bit must be set for the HFS
        /// Plus Catalog B-tree, and cleared for the HFS Plus Extents B-tree.
        /// </summary>
        VariableIndexKeys = 4,
    }
}
