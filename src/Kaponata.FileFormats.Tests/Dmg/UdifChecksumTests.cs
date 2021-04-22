// <copyright file="UdifChecksumTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="UdifChecksum"/> class.
    /// </summary>
    public class UdifChecksumTests
    {
        /// <summary>
        /// <see cref="UdifChecksum.ReadFrom(byte[], int)"/> works.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var buffer = File.ReadAllBytes("Dmg/compressedblock.bin");

            var checksum = new UdifChecksum();
            checksum.ReadFrom(buffer, 0x3c);
            Assert.Equal("AAAAIEoNh3YAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", Convert.ToBase64String(checksum.Data));
            Assert.Equal(2u, checksum.ChecksumSize);
            Assert.Equal(136, checksum.Size);
            Assert.Equal(0u, checksum.Type);
        }

        /// <summary>
        /// <see cref="UdifChecksum.WriteTo(byte[], int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var checksum = new UdifChecksum();
            Assert.Throws<NotImplementedException>(() => checksum.WriteTo(null, 0));
        }
    }
}
