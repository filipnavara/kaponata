// <copyright file="CompressionAttributeTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CompressionAttribute"/> class.
    /// </summary>
    public class CompressionAttributeTests
    {
        /// <summary>
        /// <see cref="CompressionAttribute.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var attribute = new CompressionAttribute();
            Assert.Throws<NotImplementedException>(() => attribute.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CompressionAttribute.ReadFrom(byte[], int)"/> correctly parses valid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            byte[] data = Convert.FromBase64String("ZnBtYwMAAACwEQAAAAAAAA==");

            var attribute = new CompressionAttribute();
            Assert.Equal(16, attribute.ReadFrom(data, 0));

            Assert.Equal(CompressionAttribute.DecmpfsMagic, attribute.CompressionMagic);
            Assert.Equal(FileCompressionType.ZlibAttribute, attribute.CompressionType);
            Assert.Equal(16, attribute.Size);
            Assert.Equal(4528u, attribute.UncompressedSize);
        }

        /// <summary>
        /// <see cref="CompressionAttribute.ReadFrom(byte[], int)"/> throws on invalid data.
        /// </summary>
        [Fact]
        public void ReadFrom_InvalidData_Throws()
        {
            var attribute = new CompressionAttribute();
            byte[] data = new byte[attribute.Size];

            Assert.Throws<InvalidDataException>(() => attribute.ReadFrom(data, 0));
        }
    }
}