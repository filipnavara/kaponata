// <copyright file="XarHeaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Xar;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.Xar
{
    /// <summary>
    /// Tests the <see cref="XarHeader"/> struct.
    /// </summary>
    public class XarHeaderTests
    {
        /// <summary>
        /// <see cref="XarHeader.WriteTo(byte[], int)"/> throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            XarHeader header = default;
            Assert.Throws<NotImplementedException>(() => header.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
