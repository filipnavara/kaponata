// <copyright file="CatalogFileInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CatalogFileInfo"/> class.
    /// </summary>
    public class CatalogFileInfoTests
    {
        /// <summary>
        /// <see cref="CatalogFileInfo.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var info = new CatalogFileInfo();
            Assert.Throws<NotImplementedException>(() => info.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CatalogFileInfo.ReadFrom(byte[], int)"/> correctly parses the presented data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            byte[] data = Convert.FromBase64String(
                "AAIAhgAAAAAABXXL00CTs9NFrx3TRa8d00W9aQAAAAAAAAAAAAAAAAAggaQAAAABAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "BXH/6dAAAAAAAAAAUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALBUAAAAAAAAAAwAVeJkAAAADAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

            var info = new CatalogFileInfo();
            Assert.Equal(248, info.ReadFrom(data, 0));

            Assert.Equal(new DateTime(2016, 4, 27, 0, 50, 49, DateTimeKind.Utc), info.AccessTime);
            Assert.Equal(new DateTime(2016, 4, 26, 23, 49, 49, DateTimeKind.Utc), info.AttributeModifyTime);
            Assert.Equal(new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc), info.BackupTime);
            Assert.Equal(new DateTime(2016, 4, 26, 23, 49, 49, DateTimeKind.Utc), info.ContentModifyTime);
            Assert.Equal(new DateTime(2016, 4, 23, 2, 51, 31, DateTimeKind.Utc), info.CreateTime);
            Assert.Collection(
                info.DataFork.Extents,
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                });
            Assert.Equal(0u, info.DataFork.LogicalSize);
            Assert.Equal(0x50, info.DataFork.Size);
            Assert.Equal(0u, info.DataFork.TotalBlocks);
            Assert.Equal(new CatalogNodeId(357835), info.FileId);
            Assert.Equal(0u, info.FileInfo.FileCreator);
            Assert.Equal(0u, info.FileInfo.FileType);
            Assert.Equal(FinderFlags.None, info.FileInfo.FinderFlags);
            Assert.Equal(0, info.FileInfo.Point.Horizontal);
            Assert.Equal(0, info.FileInfo.Point.Vertical);
            Assert.Equal(0x04, info.FileInfo.Point.Size);
            Assert.Equal(0x10, info.FileInfo.Size);
            Assert.Equal(0u, info.FileSystemInfo.DeviceId);
            Assert.Equal(UnixFileType.None, info.FileSystemInfo.FileType);
            Assert.Equal(0, info.FileSystemInfo.GroupId);
            Assert.Equal(0u, info.FileSystemInfo.Inode);
            Assert.Equal(0, info.FileSystemInfo.LinkCount);
            Assert.Equal(UnixFilePermissions.GroupRead, info.FileSystemInfo.Permissions);
            Assert.Equal(0, info.FileSystemInfo.UserId);
            Assert.Equal(134u, info.Flags);
            Assert.Equal(CatalogRecordType.FileRecord, info.RecordType);
            Assert.Collection(
                info.ResourceFork.Extents,
                e =>
                {
                    Assert.Equal(0x3u, e.BlockCount);
                    Assert.Equal(0x157899u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                },
                e =>
                {
                    Assert.Equal(0u, e.BlockCount);
                    Assert.Equal(0u, e.StartBlock);
                });
            Assert.Equal(11285u, info.ResourceFork.LogicalSize);
            Assert.Equal(0x50, info.ResourceFork.Size);
            Assert.Equal(3u, info.ResourceFork.TotalBlocks);
            Assert.Equal(0xf8, info.Size);
            Assert.Equal(0x81a40000u, info.UnixSpecialField);
        }
}
}
