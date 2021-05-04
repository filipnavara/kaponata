// <copyright file="ExtentKeyTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="ExtentKey"/> class.
    /// </summary>
    public class ExtentKeyTests
    {
        /// <summary>
        /// <see cref="ExtentKey.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var key = new ExtentKey();
            Assert.Throws<NotImplementedException>(() => key.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="ExtentKey.ReadFrom(byte[], int)"/> correctly parses data.
        /// </summary>
        [Fact]
        public void ReadFrom_Reads()
        {
            byte[] data = Convert.FromBase64String("AAoAAAAFNf4AAABb");

            var key = new ExtentKey();
            Assert.Equal(12, key.ReadFrom(data, 0));

            Assert.Equal(new CatalogNodeId(341502), key.NodeId);
            Assert.Equal(12, key.Size);

            Assert.Equal("ExtentKey (341502 - 91)", key.ToString());
        }

        /// <summary>
        /// Tests the <see cref="ExtentKey.CompareTo(ExtentKey)"/> method.
        /// </summary>
        /// <param name="nodeId">
        /// The node id of the first key.
        /// </param>
        /// <param name="otherNodeId">
        /// The node id of the second key.
        /// </param>
        /// <param name="startBlock">
        /// The start block of the first key.
        /// </param>
        /// <param name="otherStartBlock">
        /// The start block of the second key.
        /// </param>
        /// <param name="resourceFork">
        /// Whether the first key is a resource fork.
        /// </param>
        /// <param name="otherResourceFork">
        /// Whether the other key is a resource fork.
        /// </param>
        /// <param name="expected">
        /// The expected result.
        /// </param>
        [InlineData(1, 2, 0, 0, false, false, -1)]
        [InlineData(2, 1, 0, 0, false, false, 1)]
        [InlineData(1, 1, 0, 0, true, false, 1)]
        [InlineData(1, 1, 0, 0, false, true, -1)]
        [InlineData(1, 1, 1, 2, true, true, -1)]
        [InlineData(1, 1, 2, 1, true, true, 1)]
        [InlineData(1, 1, 1, 1, true, true, 0)]
        [Theory]
        public void CompareTo_Works(int nodeId, int otherNodeId, int startBlock, int otherStartBlock, bool resourceFork, bool otherResourceFork, int expected)
        {
            var key = new ExtentKey(new CatalogNodeId((uint)nodeId), (uint)startBlock, resourceFork);
            var other = new ExtentKey(new CatalogNodeId((uint)otherNodeId), (uint)otherStartBlock, otherResourceFork);

            Assert.Equal(expected, key.CompareTo(other));
        }

        /// <summary>
        /// <see cref="ExtentKey.CompareTo(BTreeKey)"/> throws on null values.
        /// </summary>
        [Fact]
        public void CompareTo_Null_Throws()
        {
            var key = new ExtentKey();
            Assert.Throws<ArgumentNullException>(() => key.CompareTo((BTreeKey)null));
            Assert.Throws<ArgumentNullException>(() => key.CompareTo((ExtentKey)null));
        }

        /// <summary>
        /// The <see cref="ExtentKey.Equals(object)"/> and <see cref="ExtentKey.GetHashCode"/>
        /// methods work.
        /// </summary>
        [Fact]
        public void Equals_Works()
        {
            var a1 = new ExtentKey(new CatalogNodeId(1u), 0u, true);
            var a2 = new ExtentKey(new CatalogNodeId(1u), 0u, true);
            var b = new ExtentKey(new CatalogNodeId(2u), 0u, true);

            Assert.True(a1.Equals(a2));
            Assert.Equal(a1.GetHashCode(), a2.GetHashCode());

            Assert.True(a2.Equals(a1));
            Assert.False(a1.Equals(b));
            Assert.NotEqual(a1.GetHashCode(), b.GetHashCode());

            Assert.False(a1.Equals(null));
        }
    }
}
