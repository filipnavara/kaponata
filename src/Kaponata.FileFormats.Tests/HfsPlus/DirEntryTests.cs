// <copyright file="DirEntryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="DirEntry"/> class.
    /// </summary>
    public class DirEntryTests
    {
        private readonly byte[] folderBytes = Convert.FromBase64String(
            "AAEAAAAAAAYAAAAC00Wm29NFvYLTRb2C00W3oAAAAAAAAAAAAAAAUAAAQ/0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAACcAAAAAAAAAAA==");

        private readonly byte[] fileBytes = Convert.FromBase64String(
            "AAIAhgAAAAAABXXL00CTs9NFrx3TRa8d00W9aQAAAAAAAAAAAAAAAAAggaQAAAABAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "BXH/6dAAAAAAAAAAUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALBUAAAAAAAAAAwAVeJkAAAADAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

        /// <summary>
        /// Tests the reading of a <see cref="DirEntry"/> which represents a folder.
        /// </summary>
        [Fact]
        public void FolderEntry_Reads()
        {
            var dirEntry = new DirEntry("name", this.folderBytes);
            Assert.NotNull(dirEntry.CatalogFileInfo);
            Assert.Equal(new DateTime(2016, 4, 26, 23, 14, 35, DateTimeKind.Utc), dirEntry.CreationTimeUtc);
            Assert.Equal((FileAttributes)0, dirEntry.FileAttributes);
            Assert.Equal("name", dirEntry.FileName);
            Assert.True(dirEntry.HasVfsFileAttributes);
            Assert.True(dirEntry.HasVfsTimeInfo);
            Assert.True(dirEntry.IsDirectory);
            Assert.False(dirEntry.IsSymlink);
            Assert.Equal(new DateTime(2016, 4, 27, 00, 26, 08, DateTimeKind.Utc), dirEntry.LastAccessTimeUtc);
            Assert.Equal(new DateTime(2016, 4, 27, 00, 51, 14, DateTimeKind.Utc), dirEntry.LastWriteTimeUtc);
            Assert.Equal(new CatalogNodeId(2), dirEntry.NodeId);
            Assert.Equal("name.", dirEntry.SearchName);
            Assert.Equal(2, dirEntry.UniqueCacheId);

            Assert.Equal("name", dirEntry.ToString());
        }

        /// <summary>
        /// Tests the reading of a <see cref="DirEntry"/> which represents a file.
        /// </summary>
        [Fact]
        public void FileEntry_Reads()
        {
            var dirEntry = new DirEntry("name", this.fileBytes);
            Assert.NotNull(dirEntry.CatalogFileInfo);
            Assert.Equal(new DateTime(2016, 4, 23, 2, 51, 31, DateTimeKind.Utc), dirEntry.CreationTimeUtc);
            Assert.Equal((FileAttributes)0, dirEntry.FileAttributes);
            Assert.Equal("name", dirEntry.FileName);
            Assert.True(dirEntry.HasVfsFileAttributes);
            Assert.True(dirEntry.HasVfsTimeInfo);
            Assert.False(dirEntry.IsDirectory);
            Assert.False(dirEntry.IsSymlink);
            Assert.Equal(new DateTime(2016, 4, 27, 0, 50, 49, DateTimeKind.Utc), dirEntry.LastAccessTimeUtc);
            Assert.Equal(new DateTime(2016, 4, 26, 23, 49, 49, DateTimeKind.Utc), dirEntry.LastWriteTimeUtc);
            Assert.Equal(new CatalogNodeId(0x000575cb), dirEntry.NodeId);
            Assert.Equal("name.", dirEntry.SearchName);
            Assert.Equal(0x000575cb, dirEntry.UniqueCacheId);

            Assert.Equal("name", dirEntry.ToString());
        }

        /// <summary>
        /// Tests the <see cref="DirEntry.IsFileOrDirectory(byte[])"/> method.
        /// </summary>
        [Fact]
        public void IsFileOrDirectory()
        {
            Assert.True(DirEntry.IsFileOrDirectory(this.fileBytes));
            Assert.True(DirEntry.IsFileOrDirectory(this.fileBytes));
        }

        /// <summary>
        /// <see cref="DirEntry.DirEntry(string, byte[])"/> throws when passed invalid data.
        /// </summary>
        [Fact]
        public void InvalidEntry_Throws()
        {
            Assert.Throws<NotImplementedException>(() => new DirEntry("name", new byte[100]));
        }
    }
}
