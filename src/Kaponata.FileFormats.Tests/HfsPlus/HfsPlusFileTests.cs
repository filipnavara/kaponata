// <copyright file="HfsPlusFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using Moq;
using System;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="HfsPlusFile"/> class.
    /// </summary>
    public class HfsPlusFileTests
    {
        /// <summary>
        /// Tests the <see cref="HfsPlusFile.FileContent"/> property in a scenario where the content is marked as <see cref="FileCompressionType.ZlibAttribute"/>,
        /// at the attribute level, but not actually compressed.
        /// </summary>
        [Fact]
        public void FileContent_ZlibAttribute_Raw()
        {
            var nodeId = new CatalogNodeId(1);
            byte[] compressionAttribute = Convert.FromBase64String("AAAAEAAAAAAAAAAAAAAAGWZwbWMDAAAACAAAAAAAAAD/QVBQTD8/Pz++");

            var attributes = new Mock<BTree<AttributeKey>>(MockBehavior.Strict);
            attributes
                .Setup(a => a.Find(new AttributeKey(nodeId, "com.apple.decmpfs")))
                .Returns(compressionAttribute);

            var context = new Context()
            {
                Attributes = attributes.Object,
            };

            var catalogInfo = new CatalogFileInfo()
            {
                FileId = nodeId,
            };

            var file = new HfsPlusFile(context, nodeId, catalogInfo);

            byte[] buffer = new byte[0x20];

            Assert.Equal(8, file.FileContent.Read(0, buffer, 0, buffer.Length));
            Assert.Equal("APPL????", Encoding.UTF8.GetString(buffer, 0, 8));
        }

        /// <summary>
        /// Tests the <see cref="HfsPlusFile.FileContent"/> property in a scenario where the content is marked as <see cref="FileCompressionType.ZlibAttribute"/>,
        /// at the attribute level, and zlib-compressed.
        /// </summary>
        [Fact]
        public void FileContent_ZlibAttribute_Compressed()
        {
            var nodeId = new CatalogNodeId(1);
            byte[] compressionAttribute = Convert.FromBase64String("AAAAEAAAAAAAAAAAAAAALWZwbWMDAAAAKgAAAAAAAAB4XksqyMksLjEwuMDBAAaMjAxQBgMq4AQAjygD0wA=");

            var attributes = new Mock<BTree<AttributeKey>>(MockBehavior.Strict);
            attributes
                .Setup(a => a.Find(new AttributeKey(nodeId, "com.apple.decmpfs")))
                .Returns(compressionAttribute);

            var context = new Context()
            {
                Attributes = attributes.Object,
            };

            var catalogInfo = new CatalogFileInfo()
            {
                FileId = nodeId,
            };

            var file = new HfsPlusFile(context, nodeId, catalogInfo);

            byte[] buffer = new byte[0x40];

            Assert.Equal(42, file.FileContent.Read(0, buffer, 0, buffer.Length));
            Assert.Equal("bplist00", Encoding.UTF8.GetString(buffer, 0, 8));
        }
    }
}
