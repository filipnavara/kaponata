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
