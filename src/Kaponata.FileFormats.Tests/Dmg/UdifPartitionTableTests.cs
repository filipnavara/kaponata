// <copyright file="UdifPartitionTableTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="UdifPartitionTable"/> class.
    /// </summary>
    public class UdifPartitionTableTests
    {
        /// <summary>
        /// <see cref="UdifPartitionTable.Delete(int)"/> throws.
        /// </summary>
        [Fact]
        public void Delete_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                var partitionTable = disk.Partitions;

                Assert.Throws<NotImplementedException>(() => partitionTable.Delete(0));
            }
        }

        /// <summary>
        /// <see cref="UdifPartitionTable.CreateAligned(long, WellKnownPartitionType, bool, int)"/> throws.
        /// </summary>
        [Fact]
        public void CreateAligned_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                var partitionTable = disk.Partitions;

                Assert.Throws<NotImplementedException>(() => partitionTable.CreateAligned(WellKnownPartitionType.Linux, true, 0));
                Assert.Throws<NotImplementedException>(() => partitionTable.CreateAligned(0, WellKnownPartitionType.Linux, true, 0));
            }
        }

        /// <summary>
        /// <see cref="UdifPartitionTable.Create(long, WellKnownPartitionType, bool)"/> throws.
        /// </summary>
        [Fact]
        public void Create_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            {
                var partitionTable = disk.Partitions;

                Assert.Throws<NotImplementedException>(() => partitionTable.Create(0, WellKnownPartitionType.Linux, true));
                Assert.Throws<NotImplementedException>(() => partitionTable.Create(WellKnownPartitionType.Linux, true));
            }
        }
    }
}
