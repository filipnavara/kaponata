// <copyright file="CompressedRunTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="CompressedRun"/> class.
    /// </summary>
    public class CompressedRunTests
    {
        /// <summary>
        /// <see cref="CompressedRun.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var run = new CompressedRun();
            run.ReadFrom(File.ReadAllBytes("Dmg/compressedblock.bin"), 0xcc);

            Assert.Equal(0x54, run.CompLength);
            Assert.Equal(0, run.CompOffset);
            Assert.Equal(1, run.SectorCount);
            Assert.Equal(0, run.SectorStart);
            Assert.Equal(40, run.Size);
            Assert.Equal(RunType.LzmaCompressed, run.Type);
        }

        /// <summary>
        /// <see cref="CompressedRun.WriteTo(byte[], int)"/> throws a <see cref="NotImplementedException"/>.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            var run = new CompressedRun();
            Assert.Throws<NotImplementedException>(() => run.WriteTo(null, 0));
        }
    }
}
