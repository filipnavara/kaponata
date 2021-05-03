// <copyright file="HfsPlusDirectoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.HfsPlus;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="Directory"/> class.
    /// </summary>
    public class HfsPlusDirectoryTests
    {
        /// <summary>
        /// Indirectly tests the <see cref="HfsPlusDirectory.AllEntries"/> property.
        /// </summary>
        [Fact]
        public void AllEntries_List()
        {
            using (Stream stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (HfsPlusFileSystem hfs = new HfsPlusFileSystem(stream))
            {
                var dir = hfs.GetDirectoryInfo("\\");

                Assert.Collection(
                    dir.GetFileSystemInfos(),
                    e =>
                    {
                        var dir = Assert.IsType<DiscFileSystemInfo>(e);
                        Assert.Equal(".HFS+ Private Directory Data\r", dir.Name);
                    },
                    e =>
                    {
                        var file = Assert.IsType<DiscFileSystemInfo>(e);
                        Assert.Equal("hello.txt", file.Name);
                    },
                    e =>
                    {
                        var dir = Assert.IsType<DiscFileSystemInfo>(e);
                        Assert.Equal("\0\0\0\0HFS+ Private Data", dir.Name);
                    });
            }
        }

        /// <summary>
        /// Indirectly tests the <see cref="HfsPlusDirectory.GetEntryByName(string)"/> method.
        /// </summary>
        [Fact]
        public void GetEntryByName_ReturnsEntry()
        {
            using (Stream stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (HfsPlusFileSystem hfs = new HfsPlusFileSystem(stream))
            {
                var file = hfs.GetFileInfo("\\hello.txt");
                Assert.Equal(14, file.Length);
            }
        }
    }
}
