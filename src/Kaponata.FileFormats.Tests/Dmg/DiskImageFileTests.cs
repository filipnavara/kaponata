// <copyright file="DiskImageFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using DiscUtils.Streams;
using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="DiskImageFile"/> class.
    /// </summary>
    public class DiskImageFileTests
    {
        /// <summary>
        /// The <see cref="DiskImageFile"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DiskImageFile(null, Ownership.Dispose));
        }

        /// <summary>
        /// The <see cref="DiskImageFile"/> constructor throws an exception when the stream is empty.
        /// </summary>
        [Fact]
        public void Constructor_EmptyStream_Throws()
        {
            Assert.Throws<InvalidDataException>(() => new DiskImageFile(Stream.Null, Ownership.None));
        }

        /// <summary>
        /// The <see cref="DiskImageFile"/> constructor throws an exception when the stream is
        /// not a .dmg stream.
        /// </summary>
        [Fact]
        public void Constructor_InvalidStream_Throws()
        {
            byte[] buffer = new byte[1024];

            using (Stream stream = new MemoryStream(buffer))
            {
                Assert.Throws<InvalidDataException>(() => new DiskImageFile(stream, Ownership.None));
            }
        }

        /// <summary>
        /// The default properties work correctly.
        /// </summary>
        [Fact]
        public void Properties_Work()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                Assert.NotNull(file.Buffer);
                Assert.Equal(0x2107e00, file.Capacity);
                Assert.NotNull(file.Geometry);
                Assert.True(file.IsSparse);
                Assert.False(file.NeedsParent);
                Assert.Throws<NotImplementedException>(() => file.RelativeFileLocator);
            }
        }

        /// <summary>
        /// <see cref="DiskImageFile.OpenContent(SparseStream, Ownership)"/> returns a valid stream.
        /// </summary>
        [Fact]
        public void OpenContent_Works()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            using (var content = file.OpenContent(null, Ownership.None))
            {
                var md5 = MD5.Create();
                Assert.Equal("hrgZiq8vP8ctBpxrMxyDQw==", Convert.ToBase64String(md5.ComputeHash(content)));
            }
        }

        /// <summary>
        /// <see cref="DiskImageFile.OpenContent(SparseStream, Ownership)"/> throws when the stream argument is set.
        /// </summary>
        [Fact]
        public void OpenContent_WithParentStream_Throws()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                Assert.Throws<ArgumentException>(() => file.OpenContent(SparseStream.FromStream(Stream.Null, Ownership.None), Ownership.Dispose));
            }
        }

        /// <summary>
        /// <see cref="DiskImageFile.GetParentLocations()"/> returns an empty array.
        /// </summary>
        [Fact]
        public void ParentLocations_Empty()
        {
            using (Stream stream = File.OpenRead("TestAssets/ipod-fat32.dmg"))
            using (DiskImageFile file = new DiskImageFile(stream, Ownership.None))
            {
                Assert.Empty(file.GetParentLocations());
            }
        }

        /// <summary>
        /// <see cref="DiskImageFile"/> disposes of the parent stream.
        /// </summary>
        [Fact]
        public void DisposesOfParentStream()
        {
            var stream = File.OpenRead("TestAssets/ipod-fat32.dmg");

            using (DiskImageFile file = new DiskImageFile(stream, Ownership.Dispose))
            {
                // Do nothing; just dispose
            }

            Assert.Throws<ObjectDisposedException>(() => stream.Position);
        }
    }
}
