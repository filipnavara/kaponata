// <copyright file="BTreeLeafRecordTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeLeafRecord{TKey}"/> class.
    /// </summary>
    public class BTreeLeafRecordTests
    {
        /// <summary>
        /// <see cref="BTreeLeafRecord{TKey}.ReadFrom(byte[], int)"/> correctly reads data.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var record = new BTreeLeafRecord<CatalogKey>(20);
            Assert.Equal(20, record.ReadFrom(Convert.FromBase64String("AAYAAAACAAAAAwAAAAAAAQAFAFgAYwBvAGQAZQ=="), 0));
            Assert.Equal(new CatalogNodeId(2), record.Key.NodeId);
            Assert.Equal(20, record.Size);
        }

        /// <summary>
        /// <see cref="BTreeLeafRecord{TKey}.WriteTo(byte[], int)"/> throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var record = new BTreeLeafRecord<CatalogKey>(20);
            Assert.Throws<NotImplementedException>(() => record.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
