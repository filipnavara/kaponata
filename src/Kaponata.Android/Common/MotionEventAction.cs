// <copyright file="MotionEventAction.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Common
{
    /// <summary>
    /// Motion event actions. Matches the <c>AMOTION_EVENT_ACTION_</c> values.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/frameworks/native/+/master/include/android/input.h"/>
    public enum MotionEventAction : byte
    {
        /// <summary>
        /// A pressed gesture has started, the motion contains the initial starting location.
        /// </summary>
        DOWN = 0,

        /// <summary>
        /// A pressed gesture has finished, the motion contains the final release location
        /// as well as any intermediate points since the last down or move event.
        /// </summary>
        UP = 1,

        /// <summary>
        /// A change has happened during a press gesture (between DOWN and
        /// UP).  The motion contains the most recent point, as well as
        /// any intermediate points since the last down or move event.
        /// </summary>
        MOVE = 2,

        /// <summary>
        /// The current gesture has been aborted.
        /// You will not receive any more points in it.  You should treat this as
        /// an up event, but not perform any action that you normally would.
        /// </summary>
        CANCEL = 3,

        /// <summary>
        /// A movement has happened outside of the normal bounds of the UI element.
        /// This does not provide a full gesture, but only the initial location of the movement/touch.
        /// </summary>
        OUTSIDE = 4,

        /// <summary>
        /// A non-primary pointer has gone down.
        /// The bits in POINTER_INDEX_MASK indicate which pointer changed.
        /// </summary>
        POINTER_DOWN = 5,

        /// <summary>
        /// A non-primary pointer has gone up.
        /// The bits in POINTER_INDEX_MASK indicate which pointer changed.
        /// </summary>
        POINTER_UP = 6,

        /// <summary>
        /// A change happened but the pointer is not down (unlike MOVE).
        /// The motion contains the most recent point, as well as any intermediate points since
        /// the last hover move event.
        /// </summary>
        HOVER_MOVE = 7,

        /// <summary>
        /// The motion event contains relative vertical and/or horizontal scroll offsets.
        ///  Use getAxisValue to retrieve the information from AMOTION_EVENT_AXIS_VSCROLL
        ///  and AMOTION_EVENT_AXIS_HSCROLL.
        ///  The pointer may or may not be down when this event is dispatched.
        ///  This action is always delivered to the winder under the pointer, which
        ///  may not be the window currently touched.
        /// </summary>
        SCROLL = 8,

        /// <summary>
        /// The pointer is not down but has entered the boundaries of a window or view.
        /// </summary>
        HOVER_ENTER = 9,

        /// <summary>
        /// The pointer is not down but has exited the boundaries of a window or view.
        /// </summary>
        HOVER_EXIT = 10,

        /// <summary>
        /// One or more buttons have been pressed.
        /// </summary>
        BUTTON_PRESS = 11,

        /// <summary>
        /// One or more buttons have been released.
        /// </summary>
        BUTTON_RELEASE = 12,
    }
}
