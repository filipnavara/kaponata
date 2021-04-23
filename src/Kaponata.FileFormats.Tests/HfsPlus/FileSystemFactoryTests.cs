// <copyright file="FileSystemFactoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.HfsPlus;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="FileSystemFactory"/> class.
    /// </summary>
    public class FileSystemFactoryTests
    {
        /// <summary>
        /// <see cref="FileSystemFactory.Detect(Stream, VolumeInfo)"/> returns an empty array when the
        /// <see cref="Stream"/> does not represent a HFS+ volume.
        /// </summary>
        [Fact]
        public void Detect_Invalid_ReturnsEmpty()
        {
            var factory = new FileSystemFactory();

            Assert.Empty(factory.Detect(null, null));
            Assert.Empty(factory.Detect(Stream.Null, null));
        }

        /// <summary>
        /// <see cref="FileSystemFactory.Detect(Stream, VolumeInfo)"/> returns correct information.
        /// </summary>
        [Fact]
        public void Detect_ReturnsInfo()
        {
            var factory = new FileSystemFactory();

            using (Stream stream = File.OpenRead("TestAssets/hfsp.cdr"))
            {
                var info = Assert.Single(factory.Detect(stream, null));

                Assert.Equal("Apple HFS+", info.Description);
                Assert.Equal("HFS+", info.Name);

                using (var fs = info.Open(stream))
                {
                    Assert.IsType<HfsPlusFileSystem>(fs);
                }
            }
        }
    }
}
