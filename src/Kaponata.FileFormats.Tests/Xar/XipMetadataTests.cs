// <copyright file="XipMetadataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.FileFormats.Xip;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.Xip
{
    /// <summary>
    /// Tests the <see cref="XipMetadata"/> class.
    /// </summary>
    public class XipMetadataTests
    {
        /// <summary>
        /// <see cref="XipMetadata.ReadFrom(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void ReadFrom_ValidatesArguments()
        {
            var metadata = new XipMetadata();
            Assert.Throws<ArgumentNullException>(() => metadata.ReadFrom(null));
        }

        /// <summary>
        /// <see cref="XipMetadata.ReadFrom(NSDictionary)"/> works with metadata for a Xip file
        /// which uses Gzip compression.
        /// </summary>
        [Fact]
        public void ReadGZipFormat()
        {
            var dict = new NSDictionary();
            dict.Add("Version", 1);
            dict.Add("UncompressedSize", 14);

            var metadata = new XipMetadata();
            metadata.ReadFrom(dict);
            Assert.Null(metadata.FileSystemCompressionFormat);
            Assert.Equal(1, metadata.Version);
            Assert.Equal(14, metadata.UncompressedSize);
        }

        /// <summary>
        /// <see cref="XipMetadata.ReadFrom(NSDictionary)"/> works with metadata for a Xip file
        /// which uses bpxz compression.
        /// </summary>
        [Fact]
        public void ReadBpxzFormat()
        {
            var dict = new NSDictionary();
            dict.Add("Version", 1);
            dict.Add("UncompressedSize", 0x00000007a33b8228);
            dict.Add("FileSystemCompressionFormat", "10.10");

            var metadata = new XipMetadata();
            metadata.ReadFrom(dict);
            Assert.Equal("10.10", metadata.FileSystemCompressionFormat);
            Assert.Equal(1, metadata.Version);
            Assert.Equal(0x00000007a33b8228, metadata.UncompressedSize);
        }
    }
}
