// <copyright file="InjectTextControlMessage.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Text;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="InjectTextControlMessage"/> class.
    /// </summary>
    public partial class InjectTextControlMessage : IControlMessage
    {
        /// <inheritdoc/>
        public int BinarySize
        {
            get
            {
                return Encoding.UTF8.GetByteCount(this.Text) + 3;
            }
        }

        /// <inheritdoc/>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.c#L48"/>
        public void Write(Memory<byte> memory)
        {
            var buffer = memory.Span;
            var textBytes = Encoding.UTF8.GetBytes(this.Text);

            buffer[0] = (byte)this.Type;
            BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(1), (short)textBytes.Length);
            textBytes.CopyTo(buffer.Slice(3));
        }
    }
}
