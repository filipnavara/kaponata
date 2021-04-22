// <copyright file="DiskTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.Dmg;
using DiscUtils.Streams;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="Disk"/> class.
    /// </summary>
    public class DiskTests
    {
        /// <summary>
        /// The <see cref="Disk"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new Disk(null, Ownership.Dispose));
        }

        /// <summary>
        /// The standard <see cref="Disk"/> properties work.
        /// </summary>
        [Fact]
        public void Properties_Work()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                Assert.Equal(0x2107e00, disk.Capacity);
                Assert.Equal(VirtualDiskClass.HardDisk, disk.DiskClass);
                Assert.Equal("DMG", disk.DiskTypeInfo.Name);
                Assert.Equal(string.Empty, disk.DiskTypeInfo.Variant);
                Assert.True(disk.DiskTypeInfo.CanBeHardDisk);
                Assert.True(disk.DiskTypeInfo.DeterministicGeometry);
                Assert.False(disk.DiskTypeInfo.PreservesBiosGeometry);
                Assert.NotNull(disk.DiskTypeInfo.CalcGeometry);
                Assert.NotNull(disk.Geometry);
                var layer = Assert.Single(disk.Layers);
                Assert.IsType<DiskImageFile>(layer);
            }
        }

        /// <summary>
        /// The <see cref="Disk.Partitions"/> property returns correct values.
        /// </summary>
        [Fact]
        public void PartitionTable_IsValid()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                var partitionTable = Assert.IsType<UdifPartitionTable>(disk.Partitions);
                Assert.Collection(
                    partitionTable.Partitions,
                    partition =>
                    {
                        Assert.Equal(0, partition.BiosType);
                        Assert.Equal(0, partition.FirstSector);
                        Assert.Equal(Guid.Empty, partition.GuidType);
                        Assert.Equal(1, partition.LastSector);
                        Assert.Equal(1, partition.SectorCount);
                        Assert.Equal(typeof(UdifPartitionInfo).FullName, partition.TypeAsString);
                        Assert.Equal(PhysicalVolumeType.ApplePartition, partition.VolumeType);
                    },
                    partition =>
                    {
                        Assert.Equal(0, partition.BiosType);
                        Assert.Equal(1, partition.FirstSector);
                        Assert.Equal(Guid.Empty, partition.GuidType);
                        Assert.Equal(0x1083f, partition.LastSector);
                        Assert.Equal(0x1083e, partition.SectorCount);
                        Assert.Equal(typeof(UdifPartitionInfo).FullName, partition.TypeAsString);
                        Assert.Equal(PhysicalVolumeType.ApplePartition, partition.VolumeType);
                    });
            }
        }

        /// <summary>
        /// <see cref="Disk.CreateDifferencingDisk(string)"/> throws.
        /// </summary>
        [Fact]
        public void CreateDifferencingDisk_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                Assert.Throws<NotSupportedException>(() => disk.CreateDifferencingDisk(null));
                Assert.Throws<NotSupportedException>(() => disk.CreateDifferencingDisk(null, null));
            }
        }
    }
}
