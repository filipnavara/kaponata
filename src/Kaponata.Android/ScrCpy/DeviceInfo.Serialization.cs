// <copyright file="DeviceInfo.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Text;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Supports reading and writing <see cref="DeviceInfo"/> values.
    /// </summary>
    public partial struct DeviceInfo
    {
        /// <summary>
        /// Reads a <see cref="DeviceInfo"/> value from a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer from which to read the <see cref="DeviceInfo"/> value.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceInfo"/> value.
        /// </returns>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/device_msg.c"/>
        public static DeviceInfo Read(Span<byte> buffer)
        {
            return new DeviceInfo()
            {
                DeviceName = Encoding.UTF8.GetString(buffer.Slice(0, 64)).Trim('\0'),
                Height = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(64, 2)),
                Width = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(66, 2)),
            };
        }
    }
}
