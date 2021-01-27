// <copyright file="InjectTextControlMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// A control message which can be used to inject text.
    /// </summary>
    public partial class InjectTextControlMessage
    {
        private const int ControlMsgTextMaxLength = 300;

        private string text;

        /// <inheritdoc/>
        public ControlMessageType Type { get; } = ControlMessageType.INJECT_TEXT;

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value.Substring(0, Math.Min(value.Length, ControlMsgTextMaxLength));
            }
        }
    }
}
