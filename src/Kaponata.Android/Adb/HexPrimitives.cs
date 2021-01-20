// <copyright file="HexPrimitives.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains methods to serialize/deserialize hex strings.
    /// </summary>
    public class HexPrimitives
    {
        private static readonly byte[] ReverseHexDigits = BuildReverseHexDigits();
        private static readonly byte[] HexAlphabet = new byte[] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' };

        /// <summary>
        /// Writes the value to the memory in hex string format.
        /// </summary>
        /// <param name="value">
        /// The value to be written.
        /// </param>
        /// <param name="memory">
        /// The destination memory.
        /// </param>
        public static unsafe void WriteUInt16(ushort value, Span<byte> memory)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);

            var b = buffer[0];
            memory[0] = HexAlphabet[(int)(b >> 4)];
            memory[1] = HexAlphabet[(int)(b & 0xF)];

            b = buffer[1];
            memory[2] = HexAlphabet[(int)(b >> 4)];
            memory[3] = HexAlphabet[(int)(b & 0xF)];
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> from the memory in hex string format.
        /// </summary>
        /// <param name="memory">
        /// The memory containing the value in hex string format.
        /// </param>
        /// <returns>
        /// The <see cref="ushort"/> as being read from memory.
        /// </returns>
        public static ushort ReadUShort(Span<byte> memory)
        {
            if (memory.Length < 4)
            {
                throw new ArgumentException("The memory should contain 4 bytes.");
            }

            Span<byte> result = new byte[2];

            for (int i = 0; i < 4; i++)
            {
                int c1 = ReverseHexDigits[memory[i++] - '0'] << 4;
                int c2 = ReverseHexDigits[memory[i] - '0'];

                result[i >> 1] = (byte)(c1 + c2);
            }

            return BinaryPrimitives.ReadUInt16BigEndian(result);
        }

        private static byte[] BuildReverseHexDigits()
        {
            var bytes = new byte['f' - '0' + 1];

            for (int i = 0; i < 10; i++)
            {
                bytes[i] = (byte)i;
            }

            for (int i = 10; i < 16; i++)
            {
                bytes[i + 'a' - '0' - 0x0a] = (byte)i;
                bytes[i + 'A' - '0' - 0x0a] = (byte)i;
            }

            return bytes;
        }
    }
}
