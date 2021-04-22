// <copyright file="UdifBufferTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="UdifBuffer"/> class.
    /// </summary>
    public class UdifBufferTests
    {
        /// <summary>
        /// The <see cref="UdifBuffer"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new UdifBuffer(null, new ResourceFork(new List<Resource>() { }), 0));
            Assert.Throws<ArgumentNullException>(() => new UdifBuffer(Stream.Null, null, 0));
        }

        /// <summary>
        /// The <see cref="UdifBuffer"/> properties work correctly.
        /// </summary>
        [Fact]
        public void Properties_Work()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                var buffer = file.Buffer;

                Assert.Equal(2, buffer.Blocks.Count);
                Assert.True(buffer.CanRead);
                Assert.False(buffer.CanWrite);
                Assert.Equal(0x2107e00, buffer.Capacity);
            }
        }

        /// <summary>
        /// <see cref="UdifBuffer.Write(long, byte[], int, int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                var buffer = file.Buffer;

                Assert.Throws<NotSupportedException>(() => buffer.Write(0, Array.Empty<byte>(), 0, 0));
            }
        }

        /// <summary>
        /// <see cref="UdifBuffer.SetCapacity(long)"/> throws.
        /// </summary>
        [Fact]
        public void SetCapacity_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                var buffer = file.Buffer;

                Assert.Throws<NotSupportedException>(() => buffer.SetCapacity(0));
            }
        }

        /// <summary>
        /// <see cref="UdifBuffer.GetExtentsInRange(long, long)"/> works.
        /// </summary>
        [Fact]
        public void GetExtentsInRange_Works()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                var buffer = file.Buffer;

                var extent = Assert.Single(buffer.GetExtentsInRange(5, 10));
                Assert.Equal(5, extent.Start);
                Assert.Equal(10, extent.Length);
            }
        }
    }
}
