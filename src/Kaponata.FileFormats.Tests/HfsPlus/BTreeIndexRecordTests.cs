// <copyright file="BTreeIndexRecordTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeIndexRecord{TKey}"/> class.
    /// </summary>
    public class BTreeIndexRecordTests
    {
        /// <summary>
        /// <see cref="BTreeIndexRecord{TKey}.ReadFrom(byte[], int)"/> correctly parses data.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            BTreeIndexRecord<CatalogKey> record = new BTreeIndexRecord<CatalogKey>(0x16);
            Assert.Equal(0x16, record.ReadFrom(Convert.FromBase64String("ABAAAAABAAUAWABjAG8AZABlAAA3Zg=="), 0));
            Assert.Equal(0x3766u, record.ChildId);
            Assert.Equal(new CatalogNodeId(1), record.Key.NodeId);
            Assert.Equal(0x16, record.Size);
            Assert.Equal("Xcode (1):14182", record.ToString());
        }

        /// <summary>
        /// <see cref="BTreeIndexRecord{TKey}.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            BTreeIndexRecord<CatalogKey> record = new BTreeIndexRecord<CatalogKey>(0x16);
            Assert.Throws<NotImplementedException>(() => record.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
