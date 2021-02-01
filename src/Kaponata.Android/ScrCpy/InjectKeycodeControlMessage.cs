// <copyright file="InjectKeycodeControlMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// A control message which can be used to inject an individual key event.
    /// </summary>
    public partial class InjectKeycodeControlMessage
    {
        /// <inheritdoc/>
        public ControlMessageType Type { get; } = ControlMessageType.INJECT_KEYCODE;

        /// <summary>
        /// Gets or sets the key action (up or down).
        /// </summary>
        public KeyEventAction Action { get; set; }

        /// <summary>
        /// Gets or sets the key code which uniquely identifies the key.
        /// </summary>
        public KeyCode KeyCode { get; set; }

        /// <summary>
        /// Gets or sets the key metastate (shift pressed, ctrl pressed, ...).
        /// </summary>
        public Metastate Metastate { get; set; }
    }
}
