// <copyright file="UdifResourceFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="UdifResourceFile"/> class.
    /// </summary>
    public class UdifResourceFileTests
    {
        /// <summary>
        /// <see cref="UdifResourceFile.WriteTo(byte[], int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var file = new UdifResourceFile();
            Assert.Throws<NotImplementedException>(() => file.WriteTo(null, 0));
        }
    }
}
