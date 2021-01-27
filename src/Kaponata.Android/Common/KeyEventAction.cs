// <copyright file="KeyEventAction.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Key event actions.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/frameworks/native/+/master/include/android/input.h"/>
    public enum KeyEventAction : byte
    {
        /// <summary>
        /// The key has been pressed down.
        /// </summary>
        DOWN = 0,

        /// <summary>
        /// The key has been released.
        /// </summary>
        UP = 1,

        /// <summary>
        /// Multiple duplicate key events have occurred in a row, or a
        /// complex string is being delivered.  The repeat_count property
        /// of the key event contains the number of times the given key
        /// code should be executed.
        /// </summary>
        AKEY_EVENT_ACTION_MULTIPLE = 2,
    }
}
