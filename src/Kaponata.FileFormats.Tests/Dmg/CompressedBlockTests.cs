// <copyright file="CompressedBlockTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="CompressedBlock"/> class.
    /// </summary>
    public class CompressedBlockTests
    {
        /// <summary>
        /// <see cref="CompressedBlock.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var block = new CompressedBlock();
            block.ReadFrom(File.ReadAllBytes("Dmg/compressedblock.bin"), 0);

            Assert.Equal(0u, block.BlocksDescriptor);
            Assert.Equal(2u, block.CheckSum.ChecksumSize);
            Assert.Equal("AAAAIEoNh3YAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", Convert.ToBase64String(block.CheckSum.Data));
            Assert.Equal(0x88, block.CheckSum.Size);
            Assert.Equal(0u, block.CheckSum.Type);
            Assert.Equal(0u, block.DataStart);
            Assert.Equal(0x802u, block.DecompressBufferRequested);
            Assert.Equal(0, block.FirstSector);
            Assert.Equal(1u, block.InfoVersion);
            Assert.Collection(
                block.Runs,
                r => { Assert.Equal(RunType.LzmaCompressed, r.Type); },
                r => { Assert.Equal(RunType.Terminator, r.Type); });
            Assert.Equal(1, block.SectorCount);
            Assert.Equal(0x6d697368u, block.Signature);
        }

        /// <summary>
        /// <see cref="CompressedBlock.WriteTo(byte[], int)"/> throws a <see cref="NotImplementedException"/>.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            var block = new CompressedBlock();
            Assert.Throws<NotImplementedException>(() => block.WriteTo(null, 0));
            Assert.Throws<NotImplementedException>(() => block.Size);
        }
    }
}
