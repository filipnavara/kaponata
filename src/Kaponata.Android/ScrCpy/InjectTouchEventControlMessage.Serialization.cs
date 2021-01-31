// <copyright file="InjectTouchEventControlMessage.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Text;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="InjectTouchEventControlMessage"/> class.
    /// </summary>
    public partial class InjectTouchEventControlMessage : IControlMessage
    {
        /// <inheritdoc/>
        public int BinarySize
        {
            get
            {
                return 28;
            }
        }

        /// <inheritdoc/>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/control_msg.c#L54"/>
        public void Write(Memory<byte> memory)
        {
            var buffer = memory.Span;

            buffer[0] = (byte)this.Type;
            buffer[1] = (byte)this.Action;
            BinaryPrimitives.WriteInt64BigEndian(buffer.Slice(2), this.PointerId);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(10), this.X);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(14), this.Y);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(18), this.Width);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(20), this.Height);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(22), this.ToFixedPoint16(this.Pressure));
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(24), (uint)this.Buttons);
        }

        /// <summary>
        /// Converts a float to an ushort value.
        /// </summary>
        /// <param name="f">
        /// The float value.
        /// </param>
        /// <returns>
        /// The ushort value.
        /// </returns>
        public ushort ToFixedPoint16(float f)
        {
            if (f < 0.0 || f > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(f));
            }

            uint u = (uint)(f * (1 << 16));
            if (u >= 0xffff)
            {
                u = 0xffff;
            }

            return (ushort)u;
        }
    }
}
