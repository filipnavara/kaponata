// <copyright file="VolumeHeaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="VolumeHeader"/> class.
    /// </summary>
    public class VolumeHeaderTests
    {
        /// <summary>
        /// <see cref="VolumeHeader.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var header = new VolumeHeader();
            header.ReadFrom(File.ReadAllBytes("HfsPlus/header.bin"), 0);

            Assert.Equal(0x1000u, header.AllocationFile.LogicalSize);
            Assert.Equal((VolumeAttributes)0x80000100, header.Attributes);
            Assert.Equal(0xae000u, header.AttributesFile.LogicalSize);
            Assert.Equal(new DateTime(1904, 1, 1), header.BackupDate);
            Assert.Equal(0x1000u, header.BlockSize);
            Assert.Equal(0xaf000u, header.CatalogFile.LogicalSize);
            Assert.Equal(new DateTime(2021, 1, 8, 9, 49, 58, DateTimeKind.Utc), header.CheckedDate);
            Assert.Equal(new DateTime(2021, 1, 8, 1, 49, 58, DateTimeKind.Utc), header.CreateDate);
            Assert.Equal(0x10000u, header.DataClumpSize);
            Assert.Equal(0x1u, header.EncodingsBitmap);
            Assert.Equal(0xaf000u, header.ExtentsFile.LogicalSize);
            Assert.Equal(0x134u, header.FileCount);
            Assert.Equal(new uint[] { 0u, 0u, 0u, 0u, 0u, 0u, 0x4394662cu, 0x91caac0bu }, header.FinderInfo);
            Assert.Equal(0x9cu, header.FolderCount);
            Assert.Equal(0xda2u, header.FreeBlocks);
            Assert.True(header.IsValid);
            Assert.Equal(0u, header.JournalInfoBlock);
            Assert.Equal(0x31302e30u, header.LastMountedVersion);
            Assert.Equal(new DateTime(2021, 1, 8, 9, 50, 1, DateTimeKind.Utc), header.ModifyDate);
            Assert.Equal(0xfb0u, header.NextAllocation);
            Assert.Equal(new CatalogNodeId(480), header.NextCatalogId);
            Assert.Equal(0x10000u, header.ResourceClumpSize);
            Assert.Equal(0x482bu, header.Signature);
            Assert.Equal(0x200, header.Size);
            Assert.Equal(0u, header.StartupFile.LogicalSize);
            Assert.Equal(0x231fu, header.TotalBlocks);
            Assert.Equal(0x4u, header.Version);
            Assert.Equal(0x19du, header.WriteCount);
        }

        /// <summary>
        /// <see cref="VolumeHeader.ReadFrom(byte[], int)"/> throws on invalid data.
        /// </summary>
        [Fact]
        public void ReadFrom_Invalid_Works()
        {
            var data = new byte[0x200];
            var header = new VolumeHeader();
            Assert.Equal(0x200, header.ReadFrom(data, 0));
            Assert.False(header.IsValid);
        }

        /// <summary>
        /// <see cref="VolumeHeader.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var header = new VolumeHeader();
            Assert.Throws<NotImplementedException>(() => header.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
