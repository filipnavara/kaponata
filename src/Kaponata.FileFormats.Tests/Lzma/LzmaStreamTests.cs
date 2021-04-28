// <copyright file="LzmaStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Lzma;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Kaponata.FileFormats.Tests.Lzma
{
    /// <summary>
    /// Tests the <see cref="LzmaStream"/> struct.
    /// </summary>
    public class LzmaStreamTests
    {
        /// <summary>
        /// Tests the layout of the <see cref="LzmaStream"/> struct.
        /// </summary>
        [Fact]
        public unsafe void LayoutTest()
        {
            switch (sizeof(nint))
            {
                case 8:
                    Assert.Equal(0x0, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.NextIn)).ToInt32());
                    Assert.Equal(0x8, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.AvailIn)).ToInt32());
                    Assert.Equal(0x10, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.TotalIn)).ToInt32());
                    Assert.Equal(0x18, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.NextOut)).ToInt32());
                    Assert.Equal(0x20, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.AvailOut)).ToInt32());
                    Assert.Equal(0x28, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.TotalOut)).ToInt32());
                    Assert.Equal(0x30, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.Allocator)).ToInt32());
                    Assert.Equal(0x38, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.InternalState)).ToInt32());
                    Assert.Equal(0x40, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr1)).ToInt32());
                    Assert.Equal(0x48, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr2)).ToInt32());
                    Assert.Equal(0x50, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr3)).ToInt32());
                    Assert.Equal(0x58, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr4)).ToInt32());
                    Assert.Equal(0x60, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.SeekPos)).ToInt32());
                    Assert.Equal(0x68, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt2)).ToInt32());
                    Assert.Equal(0x70, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt3)).ToInt32());
                    Assert.Equal(0x78, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt4)).ToInt32());
                    Assert.Equal(0x80, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedEnum1)).ToInt32());
                    Assert.Equal(0x84, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedEnum2)).ToInt32());

                    Assert.Equal(0x88, Marshal.SizeOf<LzmaStream>());
                    break;

                case 4:
                    Assert.Equal(0x0, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.NextIn)).ToInt32());
                    Assert.Equal(0x4, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.AvailIn)).ToInt32());
                    Assert.Equal(0x8, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.TotalIn)).ToInt32());
                    Assert.Equal(0x10, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.NextOut)).ToInt32());
                    Assert.Equal(0x14, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.AvailOut)).ToInt32());
                    Assert.Equal(0x18, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.TotalOut)).ToInt32());
                    Assert.Equal(0x20, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.Allocator)).ToInt32());
                    Assert.Equal(0x24, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.InternalState)).ToInt32());
                    Assert.Equal(0x28, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr1)).ToInt32());
                    Assert.Equal(0x2c, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr2)).ToInt32());
                    Assert.Equal(0x30, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr3)).ToInt32());
                    Assert.Equal(0x34, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedPtr4)).ToInt32());
                    Assert.Equal(0x38, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.SeekPos)).ToInt32());
                    Assert.Equal(0x40, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt2)).ToInt32());
                    Assert.Equal(0x48, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt3)).ToInt32());
                    Assert.Equal(0x4c, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedInt4)).ToInt32());
                    Assert.Equal(0x50, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedEnum1)).ToInt32());
                    Assert.Equal(0x54, Marshal.OffsetOf<LzmaStream>(nameof(LzmaStream.ReservedEnum2)).ToInt32());

                    Assert.Equal(0x58, Marshal.SizeOf<LzmaStream>());
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
