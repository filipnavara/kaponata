// <copyright file="HfsPlusFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using Moq;
using System;
using System.IO;
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

        /// <summary>
        /// Tests the <see cref="HfsPlusFile.FileContent"/> property in a scenario where the content is marked as <see cref="FileCompressionType.ZlibAttribute"/>,
        /// at the resource level, and zlib-compressed.
        /// </summary>
        [Fact]
        public void FileContent_ZlibResource()
        {
            var nodeId = new CatalogNodeId(1);
            byte[] compressionAttribute = Convert.FromBase64String("AAAAEAAAAAAAAAAAAAAAEGZwbWMEAAAAowkAAAAAAAA=");
            byte[] resourceData = Convert.FromBase64String(
                "AAABAAAAAwEAAAIBAAAAMgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAf0BAAAADAAAAPEBAAB4Xu2UUU" +
                "/bMBDHn+mnyCIeYFLtwoQ0TSGItUVUCiMiRdokNMnER+rh2JbtECrlw+MksLZTy+jUiZc++eL/" +
                "3+dfTucLTh5z7j2ANkyKY/8A9XwPRCopE9mxfz0+6372T8JO8GFw2R//iIee4sxYL77+Go36nt" +
                "/F+FQpDhgPxgMvjkbJ2HM5MB5+8z1/Yq36gnFZlojULpTKvDYaHGupQNtp5JJ13QFELfXdNW32" +
                "BRy3S1lqw85OcA/T8I5xMAGuQ7dTK3hBOlyu6eLPY25tpZ9XYGShUzD4xbATWF0AXmZBH28QV1" +
                "r+mjO/pGvNMmf2tzaXqVVLYNlkXtdAeHjQC3AT1OnwKrzZ3Vymz0VCTcl2Nwnz6c0wgt3ilBNj" +
                "wDQfqzGUdayEr4tytA4JJZbcIAoKBHU9PH1fGibu5DsWZSgy1xiTvzbrPzIcvtIj/68zlz6TWV" +
                "Cbm4e+OARmZHtnmuRQSn1vqmRCNNC5jZgX2Ui0a5e54HvcT0A/MFfO6hy4G1imuiDpZVJF7FYT" +
                "PcV7p4WVObFSV4mSltfMVSQzJkYWcrO/v7LuAowFuu7/99CbnuZ2jG3H2KZotmNs85356hibrc" +
                "3FYecJtZvhhT8rv3hKNgyeoDfeIYxXLdgi8vOb6K85mMnv728xXs0VOVtEfrxsegAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAcADIAAGNtcGYAAAAKAAH//wAAAAAAAAAA");

            var attributes = new Mock<BTree<AttributeKey>>(MockBehavior.Strict);
            attributes
                .Setup(a => a.Find(new AttributeKey(nodeId, "com.apple.decmpfs")))
                .Returns(compressionAttribute);

            var context = new Context()
            {
                Attributes = attributes.Object,
                VolumeStream = new MemoryStream(resourceData),
                VolumeHeader = new VolumeHeader()
                {
                    BlockSize = 0x00001000,
                },
            };

            var catalogInfo = new CatalogFileInfo()
            {
                FileId = nodeId,
                ResourceFork = new ForkData()
                {
                    LogicalSize = 0x0000000000000333,
                    Extents = new ExtentDescriptor[]
                    {
                        new ExtentDescriptor()
                        {
                             StartBlock = 0,
                             BlockCount = 1,
                        },
                    },
                },
            };

            var file = new HfsPlusFile(context, nodeId, catalogInfo);

            byte[] buffer = new byte[0x1000];

            Assert.Equal(0x9A3, file.FileContent.Read(0, buffer, 0, buffer.Length));
            Assert.Equal("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", Encoding.UTF8.GetString(buffer, 0, 38));
        }

        /// <summary>
        /// Tests the <see cref="HfsPlusFile.FileContent"/> property in a scenario where the content is marked as <see cref="FileCompressionType.RawAttribute"/>,
        /// at the attribute level.
        /// </summary>
        [Fact]
        public void FileContent_RawAttribute()
        {
            var nodeId = new CatalogNodeId(1);
            byte[] compressionAttribute = Convert.FromBase64String("AAAAEAAAAAAAAAAAAAAAGWZwbWMJAAAACAAAAAAAAADMQVBQTD8/Pz++");

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
    }
}
