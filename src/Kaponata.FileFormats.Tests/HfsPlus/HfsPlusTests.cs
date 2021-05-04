// <copyright file="HfsPlusTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System.IO;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// General tests for the HFS+ file system.
    /// </summary>
    public class HfsPlusTests
    {
        /// <summary>
        /// Reads data off a HFS+ file system.
        /// </summary>
        [Fact]
        public void ReadHfsPlusFilesystemTest()
        {
            using (Stream stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (HfsPlusFileSystem hfs = new HfsPlusFileSystem(stream))
            {
                Assert.True(hfs.FileExists("hello.txt"));

                using (Stream helloStream = hfs.OpenFile("hello.txt", FileMode.Open, FileAccess.Read))
                using (MemoryStream copyStream = new MemoryStream())
                {
                    Assert.NotEqual(0, helloStream.Length);
                    helloStream.CopyTo(copyStream);
                    Assert.Equal(helloStream.Length, copyStream.Length);

                    Assert.Equal("Hello, World!\n", Encoding.UTF8.GetString(copyStream.ToArray()));
                }
            }
        }
    }
}
