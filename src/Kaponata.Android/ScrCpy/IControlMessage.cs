// <copyright file="IControlMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// A common interface for all control messages.
    /// </summary>
    public interface IControlMessage
    {
        /// <summary>
        /// Gets the control message type.
        /// </summary>
        ControlMessageType Type { get; }

        /// <summary>
        /// Gets the binary size of this control message.
        /// </summary>
        int BinarySize { get; }

        /// <summary>
        /// Writes this <see cref="IControlMessage"/> value to a buffer.
        /// </summary>
        /// <param name="memory">
        /// The memory used to write the message.
        /// </param>
        public void Write(Memory<byte> memory);
    }
}
