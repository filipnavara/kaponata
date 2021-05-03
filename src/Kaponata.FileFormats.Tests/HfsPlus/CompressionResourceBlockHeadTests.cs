// <copyright file="CompressionResourceBlockHeadTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CompressionResourceBlockHead"/> class.
    /// </summary>
    public class CompressionResourceBlockHeadTests
    {
        /// <summary>
        /// <see cref="CompressionResourceBlockHead.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var head = new CompressionResourceBlockHead();
            Assert.Throws<NotImplementedException>(() => head.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CompressionResourceBlockHead.ReadFrom(byte[], int)"/> correctly parses valid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            var data = Convert.FromBase64String("AAAwSgEAAAA=");

            var head = new CompressionResourceBlockHead();
            Assert.Equal(8, head.ReadFrom(data, 0));

            Assert.Equal(12362u, head.DataSize);
            Assert.Equal(1u, head.NumBlocks);
            Assert.Equal(8, head.Size);
        }
    }
}
