// <copyright file="CatalogKeyTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CatalogKey"/> class.
    /// </summary>
    public class CatalogKeyTests
    {
        /// <summary>
        /// The <see cref="CatalogKey"/> constructor correctly initializes the properties.
        /// </summary>
        [Fact]
        public void Constructor_SetsProperties()
        {
            var key = new CatalogKey(new CatalogNodeId(1), "Xcode");
            Assert.Equal("Xcode", key.Name);
            Assert.Equal(new CatalogNodeId(1), key.NodeId);
            Assert.Equal(18, key.Size);

            Assert.Equal("Xcode (1)", key.ToString());
        }

        /// <summary>
        /// <see cref="CatalogKey.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            byte[] data = Convert.FromBase64String("ABAAAAABAAUAWABjAG8AZABl");

            CatalogKey key = new CatalogKey();
            Assert.Equal(18, key.ReadFrom(data, 0));
            Assert.Equal("Xcode", key.Name);
            Assert.Equal(new CatalogNodeId(1), key.NodeId);
            Assert.Equal(18, key.Size);
        }

        /// <summary>
        /// <see cref="CatalogKey.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var key = new CatalogKey();
            Assert.Throws<NotImplementedException>(() => key.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="CatalogKey.CompareTo(CatalogKey)"/> returns the correct value.
        /// </summary>
        /// <param name="nodeId">
        /// The node ID of the first key.
        /// </param>
        /// <param name="otherNodeId">
        /// The node ID of the second key.
        /// </param>
        /// <param name="name">
        /// The name of the first key.
        /// </param>
        /// <param name="otherName">
        /// The name of the second key.
        /// </param>
        /// <param name="result">
        /// The expected result.
        /// </param>
        [InlineData(0, 1, "a", "b", -1)]
        [InlineData(1, 0, "a", "b", 1)]
        [InlineData(1, 1, "a", "b", -1)]
        [InlineData(1, 1, "a", "a", 0)]
        [InlineData(1, 1, "b", "a", 1)]
        [Theory]
        public void CompareTo_Works(int nodeId, int otherNodeId, string name, string otherName, int result)
        {
            var key = new CatalogKey(new CatalogNodeId((uint)nodeId), name);
            var other = new CatalogKey(new CatalogNodeId((uint)otherNodeId), otherName);

            Assert.Equal(result, key.CompareTo(other));
        }

        /// <summary>
        /// <see cref="CatalogKey.CompareTo(BTreeKey)"/> throws on <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void CompareTo_Null_Throws()
        {
            var key = new CatalogKey();
            Assert.Throws<ArgumentNullException>(() => key.CompareTo((BTreeKey)null));
            Assert.Throws<ArgumentNullException>(() => key.CompareTo((CatalogKey)null));
        }
    }
}
