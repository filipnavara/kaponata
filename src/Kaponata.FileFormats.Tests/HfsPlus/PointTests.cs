// <copyright file="PointTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="Point"/> class.
    /// </summary>
    public class PointTests
    {
        /// <summary>
        /// <see cref="Point.ReadFrom(byte[], int)"/> reads the data.
        /// </summary>
        [Fact]
        public void ReadFrom_ReadsData()
        {
            var point = new Point();
            point.ReadFrom(new byte[4], 0);
            Assert.Equal(0, point.Horizontal);
            Assert.Equal(0, point.Vertical);
        }

        /// <summary>
        /// <see cref="Point.WriteTo(byte[], int)"/> throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var point = new Point();
            Assert.Throws<NotImplementedException>(() => point.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
