// <copyright file="BTreeHeaderRecordTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeHeaderRecord"/> class.
    /// </summary>
    public class BTreeHeaderRecordTests
    {
        /// <summary>
        /// <see cref="BTreeHeaderRecord.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var record = new BTreeHeaderRecord();
            byte[] buffer = Convert.FromBase64String("AAIAAAADAAADogAAAAQAAAABEAACBAAAAK8AAACGAAAACvAAAM8AAAAGAAAAAAAAAAAAAAAAAAAAAA==");

            record.ReadFrom(buffer, 0);

            Assert.Equal(BTreeAttributes.BigKeys | BTreeAttributes.VariableIndexKeys, record.Attributes);
            Assert.Equal(0xaf000u, record.ClumpSize);
            Assert.Equal(4u, record.FirstLeafNode);
            Assert.Equal(0x86u, record.FreeNodes);
            Assert.Equal(BTreeKeyCompareType.CaseFolding, record.KeyCompareType);
            Assert.Equal(0x1u, record.LastLeafNode);
            Assert.Equal(0x204u, record.MaxKeyLength);
            Assert.Equal(0x1000u, record.NodeSize);
            Assert.Equal(0x3a2u, record.NumLeafRecords);
            Assert.Equal(0u, record.Res1);
            Assert.Equal(0x3u, record.RootNode);
            Assert.Equal(0x68, record.Size);
            Assert.Equal(0xafu, record.TotalNodes);
            Assert.Equal(0x2u, record.TreeDepth);
            Assert.Equal(BTreeType.HFSBTreeType, record.TreeType);
        }

        /// <summary>
        /// <see cref="BTreeHeaderRecord.WriteTo(byte[], int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var record = new BTreeHeaderRecord();
            Assert.Throws<NotImplementedException>(() => record.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
