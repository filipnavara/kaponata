// <copyright file="HexPrimitivesTest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="HexPrimitives"/> class.
    /// </summary>
    public class HexPrimitivesTest
    {
        /// <summary>
        /// The <see cref="HexPrimitives.WriteUInt16(ushort, Span{byte})"/> writes the hex string encoded value into the memory.
        /// </summary>
        [Fact]
        public void WriteUInt16_ValueInMemory()
        {
            Span<byte> buffer = new byte[4];
            HexPrimitives.WriteUInt16(12, buffer);

            Assert.Equal((byte)'0', buffer[0]);
            Assert.Equal((byte)'0', buffer[1]);
            Assert.Equal((byte)'0', buffer[2]);
            Assert.Equal((byte)'C', buffer[3]);
        }

        /// <summary>
        /// The <see cref="HexPrimitives.ReadUShort(Span{byte})"/> reads the hex string encoded value from memory.
        /// </summary>
        [Fact]
        public void ReadUShort_ValueFromMemory()
        {
            Span<byte> buffer = new byte[]
            {
                (byte)'0',
                (byte)'0',
                (byte)'0',
                (byte)'C',
            };

            var value = HexPrimitives.ReadUShort(buffer);
            Assert.Equal(12, value);
        }

        /// <summary>
        /// The <see cref="HexPrimitives.ReadUShort(Span{byte})"/> throws an exception when the memory length is less than 4.
        /// </summary>
        [Fact]
        public void ReadUShort_ThrowsOnInvalidBuffer()
        {
            var buffer = new byte[]
            {
                (byte)'0',
                (byte)'0',
                (byte)'0',
            };

            Assert.Throws<ArgumentException>(() => HexPrimitives.ReadUShort(buffer));
        }
    }
}
