// <copyright file="BTreeNodeTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using DiscUtils.Streams;
using Kaponata.FileFormats.Lzfse;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeNode"/> class.
    /// </summary>
    public class BTreeNodeTests
    {
        private readonly IBuffer buffer = new SparseMemoryBuffer(0x1000);

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeNodeTests"/> class.
        /// </summary>
        public BTreeNodeTests()
        {
            var data = new byte[0x1000];
            LzfseCompressor.Decompress(
                Convert.FromBase64String("YnZ4MgAQAAAoAOAAAAkAECj92IkRFQAQnwAAABVcAAhn5xuAvQFgAxuA/QUAAMA+ex8+BIAPgScAgCcAAAAAAAAAAAAAAACAv/A3+A" +
                "1egBfgBQB4AYAX4AV4AQAAAAAAAAAAAAAAAAAAAIAXAAAAAAAAAAAAAHgBXgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA+BrgBQAAAJybRYhvkHVluTVKA" +
                "gEAAAAAAAAAAACPjP9nCxT9ouKyfQFidngk"),
                data);
            this.buffer.Write(0, data, 0, data.Length);

            var nodeData = new byte[0x1000];
            LzfseCompressor.Decompress(
                Convert.FromBase64String("YnZ4MgAQAAAwABABAAcAIO352ciiEQBwmQAAABiU8Am3FwBcAKcAF8AFcHFxwQUAANx+FR8DAPANfAMAAAAAAAAAAAAAAAAA/JDX19" +
                "fX11zDNfATAAAAANcAAADANQBcAwBcAwAAAABwDQDXAMA1wDUAAAAAAAAAAAAAAAAAXAMAAAAAAAAAAAAA1wAAAAAAAAAAAAAAAHANAA7JOpEyn85v7ZpAoN03DwUAA" +
                "AAAAAAAAOBS7/+VYMAs6WJ2eCQ="),
                nodeData);
            this.buffer.Write(3 * nodeData.Length, nodeData, 0, nodeData.Length);
        }

        /// <summary>
        /// <see cref="BTreeNode.ReadNode(BTree, byte[], int)"/> throws when the node is of an unsupported kind.
        /// </summary>
        /// <param name="kind">
        /// The node kind.
        /// </param>
        [InlineData(BTreeNodeKind.IndexNode)]
        [InlineData(BTreeNodeKind.MapNode)]
        [InlineData(BTreeNodeKind.LeafNode)]
        [Theory]
        public void ReadNode_ThrowsOnInvalidKind(BTreeNodeKind kind)
        {
            var descriptor = new BTreeNodeDescriptor();
            descriptor.Kind = kind;

            byte[] data = new byte[descriptor.Size];
            descriptor.WriteTo(data, 0);

            var tree = new BTree<ExtentKey>(this.buffer);
            Assert.Throws<NotImplementedException>(() => BTreeNode.ReadNode(tree, data, 0));
        }

        /// <summary>
        /// <see cref="BTreeNode.ReadNode(BTree, byte[], int)"/> throws when the node is of an unsupported kind.
        /// </summary>
        [Fact]
        public void ReadNode_Generic_ThrowsOnInvalidKind()
        {
            var descriptor = new BTreeNodeDescriptor();
            descriptor.Kind = BTreeNodeKind.MapNode;

            byte[] data = new byte[descriptor.Size];
            descriptor.WriteTo(data, 0);

            var tree = new BTree<ExtentKey>(this.buffer);
            Assert.Throws<NotImplementedException>(() => BTreeNode.ReadNode<CatalogKey>(tree, data, 0));
        }
    }
}
