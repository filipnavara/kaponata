// <copyright file="CompressionResourceHeaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CompressionResourceHeader"/> class.
    /// </summary>
    public class CompressionResourceHeaderTests
    {
        /// <summary>
        /// <see cref="CompressionResourceHeader.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var head = new CompressionResourceHeader();
            Assert.Throws<NotImplementedException>(() => head.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CompressionResourceHeader.ReadFrom(byte[], int)"/> correctly parses valid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            var data = Convert.FromBase64String("AAABAAAAMU4AADBOAAAAMg==");

            var head = new CompressionResourceHeader();
            Assert.Equal(16, head.ReadFrom(data, 0));

            Assert.Equal(12366u, head.DataSize);
            Assert.Equal(0x32u, head.Flags);
            Assert.Equal(0x100u, head.HeaderSize);
            Assert.Equal(16, head.Size);
            Assert.Equal(12622u, head.TotalSize);
        }
    }
}
