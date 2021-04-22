// <copyright file="UdifPartitionInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using DiscUtils.Streams;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="UdifPartitionInfo" /> class.
    /// </summary>
    public class UdifPartitionInfoTests
    {
        /// <summary>
        /// <see cref="UdifPartitionInfo.Open"/> returns the correct value.
        /// </summary>
        [Fact]
        public void Open_Works()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (Disk disk = new Disk(stream, Ownership.None))
            using (Stream partitionStream = disk.Partitions[0].Open())
            {
                Assert.Equal(512, partitionStream.Length);
            }
        }
    }
}
