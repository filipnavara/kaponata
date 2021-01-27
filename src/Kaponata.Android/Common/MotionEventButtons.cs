// <copyright file="MotionEventButtons.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Constants that identify buttons that are associated with motion events.
    /// Matches the <c>AMOTION_EVENT_BUTTON_</c> values.
    /// Refer to the documentation on the MotionEvent class for descriptions of each button.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/frameworks/native/+/master/include/android/input.h"/>
    public enum MotionEventButtons
    {
        /// <summary>
        /// The primary event button.
        /// </summary>
        PRIMARY = 1 << 0,

        /// <summary>
        /// The secondary event button.
        /// </summary>
        SECONDARY = 1 << 1,

        /// <summary>
        /// The tertiary event button.
        /// </summary>
        TERTIARY = 1 << 2,

        /// <summary>
        /// The back event button.
        /// </summary>
        BACK = 1 << 3,

        /// <summary>
        /// The forward event button.
        /// </summary>
        FORWARD = 1 << 4,

        /// <summary>
        /// The stylus primary event button.
        /// </summary>
        STYLUS_PRIMARY = 1 << 5,

        /// <summary>
        /// The stylus secondary event button.
        /// </summary>
        STYLUS_SECONDARY = 1 << 6,
    }
}
