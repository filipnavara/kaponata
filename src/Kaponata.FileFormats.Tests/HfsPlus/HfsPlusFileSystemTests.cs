// <copyright file="HfsPlusFileSystemTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using DiscUtils.Streams;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="HfsPlusFileSystem"/> class.
    /// </summary>
    public class HfsPlusFileSystemTests
    {
        /// <summary>
        /// The <see cref="HfsPlusFileSystem"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new HfsPlusFileSystem(null));
        }

        /// <summary>
        /// The common properties of the <see cref="HfsPlusFileSystem"/> work correctly.
        /// </summary>
        [Fact]
        public void CommonProperties_Work()
        {
            using (var stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (var hfs = new HfsPlusFileSystem(stream))
            {
                Assert.Equal("Apple HFS+", hfs.FriendlyName);
                Assert.Equal("Volume Name", hfs.VolumeLabel);
                Assert.False(hfs.CanWrite);

                Assert.Throws<NotSupportedException>(() => hfs.Size);
                Assert.Throws<NotSupportedException>(() => hfs.UsedSpace);
                Assert.Throws<NotSupportedException>(() => hfs.AvailableSpace);
            }
        }

        /// <summary>
        /// <see cref="HfsPlusFileSystem.GetUnixFileInfo(string)"/> throws when the file does not
        /// exist.
        /// </summary>
        [Fact]
        public void GetUnixFileInfo_MissingFile_Throws()
        {
            using (var stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (var hfs = new HfsPlusFileSystem(stream))
            {
                Assert.Throws<FileNotFoundException>(() => hfs.GetUnixFileInfo(@"\test\invalid"));
            }
        }

        /// <summary>
        /// <see cref="HfsPlusFileSystem.GetUnixFileInfo(string)"/> returns correct values.
        /// </summary>
        [Fact]
        public void GetUnixFileInfo_ReturnsInfo()
        {
            using (var stream = File.OpenRead("TestAssets/hfsp.cdr"))
            using (var hfs = new HfsPlusFileSystem(stream))
            {
                var fileInfo = hfs.GetFileInfo("hello.txt");

                Assert.Equal((FileAttributes)0, fileInfo.Attributes);
                Assert.True(fileInfo.CreationTimeUtc > new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                Assert.Equal("\\", fileInfo.DirectoryName);
                Assert.True(fileInfo.Exists);
                Assert.Equal("txt", fileInfo.Extension);
                Assert.Same(hfs, fileInfo.FileSystem);
                Assert.Equal("hello.txt", fileInfo.FullName);
                Assert.False(fileInfo.IsReadOnly);
                Assert.True(fileInfo.LastAccessTimeUtc > new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                Assert.True(fileInfo.LastWriteTimeUtc > new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                Assert.Equal(0xe, fileInfo.Length);
                Assert.Equal("hello.txt", fileInfo.Name);
                Assert.NotNull(fileInfo.Parent);
            }
        }

        /// <summary>
        /// The <see cref="HfsPlusFileSystem"/> constructor throws when passed an empty stream.
        /// </summary>
        [Fact]
        public void Constructor_InsufficientData_Throws()
        {
            Assert.Throws<InvalidDataException>(() => new HfsPlusFileSystem(new MemoryStream()));
        }

        /// <summary>
        /// The <see cref="HfsPlusFileSystem"/> constructor throws when passed an empty stream.
        /// </summary>
        [Fact]
        public void Constructor_InvalidData_Throws()
        {
            using (Stream stream = new ZeroStream(0x1000))
            {
                Assert.Throws<InvalidDataException>(() => new HfsPlusFileSystem(stream));
            }
        }
    }
}
