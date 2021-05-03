// <copyright file="BTreeGenericRecordTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeGenericRecord"/> class.
    /// </summary>
    public class BTreeGenericRecordTests
    {
        /// <summary>
        /// <see cref="BTreeGenericRecord.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            byte[] data = new byte[] { 1, 2, 3, 4 };
            var record = new BTreeGenericRecord(2);
            Assert.Equal(2, record.ReadFrom(data, 1));
            Assert.Equal(2, record.Size);
        }

        /// <summary>
        /// <see cref="BTreeGenericRecord.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var record = new BTreeGenericRecord(2);
            Assert.Throws<NotImplementedException>(() => record.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
