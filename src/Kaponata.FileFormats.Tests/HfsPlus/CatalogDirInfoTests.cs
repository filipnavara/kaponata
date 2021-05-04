// <copyright file="CatalogDirInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CatalogDirInfo"/> class.
    /// </summary>
    public class CatalogDirInfoTests
    {
        /// <summary>
        /// <see cref="CatalogDirInfo.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var info = new CatalogDirInfo();
            Assert.Throws<NotImplementedException>(() => info.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CatalogDirInfo.ReadFrom(byte[], int)"/> correctly parses valid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            var data = Convert.FromBase64String("AAEAAAAAAAYAAAAC00Wm29NFvYLTRb2C00W3oAAAAAAAAAAAAAAAUAAAQ/0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACcAAAAAAAAAAA==");

            var info = new CatalogDirInfo();
            Assert.Equal(52, info.ReadFrom(data, 0));

            Assert.Equal(new DateTime(2016, 4, 27, 00, 26, 08, DateTimeKind.Utc), info.AccessTime);
            Assert.Equal(new DateTime(2016, 4, 27, 00, 51, 14, DateTimeKind.Utc), info.AttributeModifyTime);
            Assert.Equal(new DateTime(1904, 1, 1, 00, 00, 00, DateTimeKind.Utc), info.BackupTime);
            Assert.Equal(new DateTime(2016, 4, 27, 00, 51, 14, DateTimeKind.Utc), info.ContentModifyTime);
            Assert.Equal(new DateTime(2016, 4, 26, 23, 14, 35, DateTimeKind.Utc), info.CreateTime);
            Assert.Equal(new CatalogNodeId(2), info.FileId);
            Assert.Equal(0u, info.FileSystemInfo.DeviceId);
            Assert.Equal(UnixFileType.None, info.FileSystemInfo.FileType);
            Assert.Equal(0x50, info.FileSystemInfo.GroupId);
            Assert.Equal(0u, info.FileSystemInfo.Inode);
            Assert.Equal(0, info.FileSystemInfo.LinkCount);
            Assert.Equal(UnixFilePermissions.None, info.FileSystemInfo.Permissions);
            Assert.Equal(0, info.FileSystemInfo.UserId);
            Assert.Equal(0, info.Flags);
            Assert.Equal(CatalogRecordType.FolderRecord, info.RecordType);
            Assert.Equal(52, info.Size);
            Assert.Equal(0x43fd0000u, info.UnixSpecialField);
            Assert.Equal(6u, info.Valence);
        }
    }
}
