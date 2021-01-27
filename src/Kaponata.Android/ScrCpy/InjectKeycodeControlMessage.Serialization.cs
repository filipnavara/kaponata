// <copyright file="InjectKeycodeControlMessage.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Text;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="InjectKeycodeControlMessage"/> class.
    /// </summary>
    public partial class InjectKeycodeControlMessage : IControlMessage
    {
        /// <inheritdoc/>
        public int BinarySize
        {
            get
            {
                return 10;
            }
        }

        /// <inheritdoc/>
        public void Write(Memory<byte> memory)
        {
            var buffer = memory.Span;
            buffer[0] = (byte)this.Type;
            buffer[1] = (byte)this.Action;
            BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(2), (int)this.KeyCode);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(6), (int)this.Metastate);
        }
    }
}
