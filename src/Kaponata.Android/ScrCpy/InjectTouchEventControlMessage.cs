// <copyright file="InjectTouchEventControlMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using System.Drawing;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// A control message which can be used to inject touch events.
    /// </summary>s
    public partial class InjectTouchEventControlMessage
    {
        /// <inheritdoc/>
        public ControlMessageType Type { get; } = ControlMessageType.INJECT_TOUCH_EVENT;

        /// <summary>
        /// Gets or sets the touch action (up, down, move...).
        /// </summary>
        public MotionEventAction Action { get; set; }

        /// <summary>
        /// Gets or sets the buttons (primary, secondary,...) to which the touch event applies.
        /// </summary>
        public MotionEventButtons Buttons { get; set; }

        /// <summary>
        /// Gets or sets the x position of the touch event.
        /// </summary>
        public uint X { get; set; }

        /// <summary>
        /// Gets or sets the y position of the touch event.
        /// </summary>
        public uint Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the screen. The server will use this to re-scale coordinates.
        /// </summary>
        public ushort Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the screen. The server will use this to re-scale coordinates.
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        /// Gets or sets the ID of the touch pointer. Use multiple IDs for multi-touch events.
        /// </summary>
        public long PointerId { get; set; }

        /// <summary>
        /// Gets or sets the pressure being applied.
        /// </summary>
        public float Pressure { get; set; }
    }
}
