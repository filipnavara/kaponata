// <copyright file="DeveloperDiskTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperDisks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.DeveloperDisks
{
    /// <summary>
    /// Tests the <see cref="DeveloperDisk"/> class.
    /// </summary>
    public class DeveloperDiskTests
    {
        /// <summary>
        /// <see cref="DeveloperDisk.FromFileAsync(Stream, Stream, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task FromFileAsync_ValidatesArguments_Async()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DeveloperDisk.FromFileAsync(null, Stream.Null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => DeveloperDisk.FromFileAsync(Stream.Null, null, default));
        }

        /// <summary>
        /// <see cref="DeveloperDisk.FromFileAsync(Stream, Stream, CancellationToken)"/> works as intended.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task FromFileAsync_Works_Async()
        {
            using (Stream stream = File.OpenRead("TestAssets/udro-hfsplus.dmg"))
            using (MemoryStream signature = new MemoryStream(new byte[] { 1, 2, 3, 4 }))
            {
                var disk = await DeveloperDisk.FromFileAsync(stream, signature, default).ConfigureAwait(false);

                Assert.Same(stream, disk.Image);
                Assert.Equal(new byte[] { 1, 2, 3, 4 }, disk.Signature);
                Assert.NotNull(disk.Version);
                Assert.Equal("17E255", disk.Version.ProductBuildVersion.ToString());
            }
        }

        /// <summary>
        /// <see cref="DeveloperDisk.Dispose" /> works correctly.
        /// </summary>
        [Fact]
        public void Dispose_Works()
        {
            var disk = new DeveloperDisk();
            disk.Dispose();
            Assert.True(disk.IsDisposed);

            disk = new DeveloperDisk();
            disk.Image = new MemoryStream();
            disk.Dispose();

            Assert.Throws<ObjectDisposedException>(() => disk.Image.Length);
        }
    }
}
