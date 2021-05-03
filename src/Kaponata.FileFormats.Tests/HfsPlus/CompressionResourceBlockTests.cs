// <copyright file="CompressionResourceBlockTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CompressionResourceBlock"/> class.
    /// </summary>
    public class CompressionResourceBlockTests
    {
        /// <summary>
        /// <see cref="CompressionResourceBlock.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var block = new CompressionResourceBlock();
            Assert.Throws<NotImplementedException>(() => block.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CompressionResourceBlock.ReadFrom(byte[], int)"/> correctly parses valid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            var data = Convert.FromBase64String("DAAAAD4wAAA=");

            var block = new CompressionResourceBlock();
            Assert.Equal(8, block.ReadFrom(data, 0));

            Assert.Equal(12350u, block.DataSize);
            Assert.Equal(12u, block.Offset);
            Assert.Equal(8, block.Size);
        }
    }
}
