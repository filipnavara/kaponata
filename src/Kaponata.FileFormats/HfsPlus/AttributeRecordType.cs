// <copyright file="AttributeRecordType.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Describes the type of attribute data record.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#AttributesFile"/>
    public enum AttributeRecordType
    {
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        InlineData = 0x10,

        /// <summary>
        /// This record is a fork data attribute. You can use the HFSPlusAttrForkData type to interpret the data.
        /// </summary>
        ForkData = 0x20,

        /// <summary>
        /// This record is an extension attribute. You can use the HFSPlusAttrExtents type to interpret the data.
        /// </summary>
        Extents = 0x30,
    }
}
