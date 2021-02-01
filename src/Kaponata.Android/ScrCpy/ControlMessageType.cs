// <copyright file="ControlMessageType.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Identifies a control message.
    /// </summary>
    /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.h"/>
    public enum ControlMessageType : byte
    {
        /// <summary>
        /// A message which injects an individual key code.
        /// </summary>
        INJECT_KEYCODE,

        /// <summary>
        /// A message which injects a string.
        /// </summary>
        INJECT_TEXT,

        /// <summary>
        /// A message which injects a touch event.
        /// </summary>
        INJECT_TOUCH_EVENT,

        /// <summary>
        /// A message which injects a scroll event.
        /// </summary>
        INJECT_SCROLL_EVENT,

        /// <summary>
        /// A message which injects a back or screen on event.
        /// </summary>
        BACK_OR_SCREEN_ON,

        /// <summary>
        /// A message which expands the notification panel.
        /// </summary>
        EXPAND_NOTIFICATION_PANEL,

        /// <summary>
        /// A message which collapes the notification panel.
        /// </summary>
        COLLAPSE_NOTIFICATION_PANEL,

        /// <summary>
        /// A message which gets the clipboard contents.
        /// </summary>
        GET_CLIPBOARD,

        /// <summary>
        /// A message which sets the clipboard contents.
        /// </summary>
        SET_CLIPBOARD,

        /// <summary>
        /// A message which sets the screen power mode.
        /// </summary>
        SET_SCREEN_POWER_MODE,

        /// <summary>
        /// A message which rotates the device.
        /// </summary>
        ROTATE_DEVICE,
    }
}
