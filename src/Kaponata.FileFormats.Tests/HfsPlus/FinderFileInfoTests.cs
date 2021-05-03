// <copyright file="FileInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="FinderFileInfo"/> class.
    /// </summary>
    public class FinderFileInfoTests
    {
        /// <summary>
        /// <see cref="FinderFileInfo.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var info = new FinderFileInfo();
            Assert.Throws<NotImplementedException>(() => info.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="FinderFileInfo.ReadFrom(byte[], int)"/> correctly parses data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            byte[] data = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAA==");

            var key = new FinderFileInfo();
            Assert.Equal(16, key.ReadFrom(data, 0));
            Assert.Equal(0u, key.FileCreator);
            Assert.Equal(0u, key.FileType);
            Assert.Equal(FinderFlags.None, key.FinderFlags);
            Assert.Equal(0, key.Point.Horizontal);
            Assert.Equal(0, key.Point.Vertical);
            Assert.Equal(16, key.Size);
        }
    }
}
