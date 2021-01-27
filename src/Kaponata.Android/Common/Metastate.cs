// <copyright file="Metastate.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Meta key / modifier state. Corresponds to the values of the <c>AMETA_</c> enum.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/frameworks/native/+/master/include/android/input.h"/>
    public enum Metastate
    {
        /// <summary>
        /// No meta keys are pressed.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// This mask is used to check whether one of the ALT meta keys is pressed.
        /// </summary>
        ALT_ON = 0x02,

        /// <summary>
        /// This mask is used to check whether the left ALT meta key is pressed.
        /// </summary>
        ALT_LEFT_ON = 0x10,

        /// <summary>
        /// This mask is used to check whether the right ALT meta key is pressed.
        /// </summary>
        ALT_RIGHT_ON = 0x20,

        /// <summary>
        /// This mask is used to check whether one of the SHIFT meta keys is pressed.
        /// </summary>
        SHIFT_ON = 0x01,

        /// <summary>
        /// This mask is used to check whether the left SHIFT meta key is pressed.
        /// </summary>
        SHIFT_LEFT_ON = 0x40,

        /// <summary>
        /// This mask is used to check whether the right SHIFT meta key is pressed.
        /// </summary>
        SHIFT_RIGHT_ON = 0x80,

        /// <summary>
        /// This mask is used to check whether the SYM meta key is pressed.
        /// </summary>
        SYM_ON = 0x04,

        /// <summary>
        /// This mask is used to check whether the FUNCTION meta key is pressed.
        /// </summary>
        FUNCTION_ON = 0x08,

        /// <summary>
        /// This mask is used to check whether one of the CTRL meta keys is pressed.
        /// </summary>
        CTRL_ON = 0x1000,

        /// <summary>
        /// This mask is used to check whether the left CTRL meta key is pressed.
        /// </summary>
        CTRL_LEFT_ON = 0x2000,

        /// <summary>
        /// This mask is used to check whether the right CTRL meta key is pressed.
        /// </summary>
        CTRL_RIGHT_ON = 0x4000,

        /// <summary>
        /// This mask is used to check whether one of the META meta keys is pressed.
        /// </summary>
        META_ON = 0x10000,

        /// <summary>
        /// This mask is used to check whether the left META meta key is pressed.
        /// </summary>
        META_LEFT_ON = 0x20000,

        /// <summary>
        /// This mask is used to check whether the right META meta key is pressed.
        /// </summary>
        META_RIGHT_ON = 0x40000,

        /// <summary>
        /// This mask is used to check whether the CAPS LOCK meta key is on.
        /// </summary>
        CAPS_LOCK_ON = 0x100000,

        /// <summary>
        /// This mask is used to check whether the NUM LOCK meta key is on.
        /// </summary>
        NUM_LOCK_ON = 0x200000,

        /// <summary>
        /// This mask is used to check whether the SCROLL LOCK meta key is on.
        /// </summary>
        SCROLL_LOCK_ON = 0x400000,
    }
}
