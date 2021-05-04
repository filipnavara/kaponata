// <copyright file="BTreeIndexNodeTests.cs" company="Quamotion bv">
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
    /// Tests the <see cref="BTreeIndexNode{TKey}"/> class.
    /// </summary>
    public class BTreeIndexNodeTests
    {
        private readonly byte[] nodeData;
        private readonly IBuffer buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeIndexNodeTests"/> class.
        /// </summary>
        public BTreeIndexNodeTests()
        {
            this.buffer = new SparseMemoryBuffer(0x1000);

            var data = new byte[0x1000];
            LzfseCompressor.Decompress(
                Convert.FromBase64String("YnZ4MgAQAAAoAOAAAAkAECj92IkRFQAQnwAAABVcAAhn5xuAvQFgAxuA/QUAAMA+ex8+BIAPgScAgCcAAAAAAAAAAAAAAACAv/A3+A" +
                "1egBfgBQB4AYAX4AV4AQAAAAAAAAAAAAAAAAAAAIAXAAAAAAAAAAAAAHgBXgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA+BrgBQAAAJybRYhvkHVluTVKA" +
                "gEAAAAAAAAAAACPjP9nCxT9ouKyfQFidngk"),
                data);
            this.buffer.Write(0, data, 0, data.Length);

            this.nodeData = new byte[0x1000];
            LzfseCompressor.Decompress(
                Convert.FromBase64String("YnZ4MgAQAAAwABABAAcAIO352ciiEQBwmQAAABiU8Am3FwBcAKcAF8AFcHFxwQUAANx+FR8DAPANfAMAAAAAAAAAAAAAAAAA/JDX19" +
                "fX11zDNfATAAAAANcAAADANQBcAwBcAwAAAABwDQDXAMA1wDUAAAAAAAAAAAAAAAAAXAMAAAAAAAAAAAAA1wAAAAAAAAAAAAAAAHANAA7JOpEyn85v7ZpAoN03DwUAA" +
                "AAAAAAAAOBS7/+VYMAs6WJ2eCQ="),
                this.nodeData);
            this.buffer.Write(3 * this.nodeData.Length, this.nodeData, 0, this.nodeData.Length);
        }

        /// <summary>
        /// <see cref="BTreeIndexNode{TKey}.FindKey(TKey)"/> returns <see langword="null"/> if the requested key was not found.
        /// </summary>
        [Fact]
        public void FindKey_NotFound_ReturnsNull()
        {
            var tree = new BTree<ExtentKey>(this.buffer);
            var indexNode = new BTreeIndexNode<ExtentKey>(tree, new BTreeNodeDescriptor() { NumRecords = 3 });
            indexNode.ReadFrom(this.nodeData, 0);

            var key = new ExtentKey(new CatalogNodeId(0), 0, false);
            Assert.Null(indexNode.FindKey(key));
        }

        /// <summary>
        /// <see cref="BTreeIndexNode{TKey}.VisitRange(BTreeVisitor{TKey})"/> works for cases where the node is not found.
        /// </summary>
        [Fact]
        public void VisitRange_NotFound_Works()
        {
            var tree = new BTree<ExtentKey>(this.buffer);
            var indexNode = new BTreeIndexNode<ExtentKey>(tree, new BTreeNodeDescriptor() { NumRecords = 3 });
            indexNode.ReadFrom(this.nodeData, 0);

            indexNode.VisitRange((key, data) => { return 1; });
        }
    }
}
