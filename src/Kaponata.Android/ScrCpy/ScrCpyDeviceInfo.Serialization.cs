// <copyright file="ScrCpyDeviceInfo.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Text;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Supports reading and writing <see cref="ScrCpyDeviceInfo"/> values.
    /// </summary>
    public partial class ScrCpyDeviceInfo
    {
        /// <summary>
        /// Gets the binary size of the device info.
        /// </summary>
        public static int BinarySize
        {
            get
            {
                return 68;
            }
        }

        /// <summary>
        /// Reads a <see cref="ScrCpyDeviceInfo"/> value from a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer from which to read the <see cref="ScrCpyDeviceInfo"/> value.
        /// </param>
        /// <returns>
        /// A <see cref="ScrCpyDeviceInfo"/> value.
        /// </returns>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/device_msg.c"/>
        public static ScrCpyDeviceInfo Read(Span<byte> buffer)
        {
            return new ScrCpyDeviceInfo()
            {
                DeviceName = Encoding.UTF8.GetString(buffer.Slice(0, 64)).Trim('\0'),
                Height = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(64, 2)),
                Width = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(66, 2)),
            };
        }
    }
}
