// <copyright file="FileBufferTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using DiscUtils.Streams;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="FileBuffer"/> class.
    /// </summary>
    public class FileBufferTests
    {
        /// <summary>
        /// The basic <see cref="FileBuffer"/> properties return the correct values.
        /// </summary>
        [Fact]
        public void BasicProperties_Work()
        {
            var buffer = new FileBuffer(
                new Context(),
                new ForkData()
                {
                    LogicalSize = 0x123,
                },
                new CatalogNodeId(1));

            Assert.True(buffer.CanRead);
            Assert.False(buffer.CanWrite);
            Assert.Equal(0x123, buffer.Capacity);
        }

        /// <summary>
        /// <see cref="FileBuffer.Write(long, byte[], int, int)"/> always throws.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            var buffer = new FileBuffer(
                new Context(),
                new ForkData(),
                new CatalogNodeId(1));

            Assert.Throws<NotSupportedException>(() => buffer.Write(1, Array.Empty<byte>(), 0, 1));
        }

        /// <summary>
        /// <see cref="FileBuffer.SetCapacity(long)"/> always throws.
        /// </summary>
        [Fact]
        public void SetCapacity_Throws()
        {
            var buffer = new FileBuffer(
               new Context(),
               new ForkData(),
               new CatalogNodeId(1));

            Assert.Throws<NotSupportedException>(() => buffer.SetCapacity(1));
        }

        /// <summary>
        /// <see cref="FileBuffer.Read(long, byte[], int, int)"/> works when all data can be
        /// read from a single extent.
        /// </summary>
        [Fact]
        public void SimpleRead_Works()
        {
            var buffer = new FileBuffer(
                new Context()
                {
                    VolumeStream =
                        SparseStream.FromStream(
                            new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!")), Ownership.Dispose),
                    VolumeHeader = new VolumeHeader()
                    {
                        BlockSize = 0x100,
                    },
                },
                new ForkData()
                {
                    LogicalSize = 0x123,
                    Extents = new ExtentDescriptor[]
                    {
                         new ExtentDescriptor()
                         {
                              BlockCount = 1,
                              StartBlock = 0,
                         },
                    },
                },
                new CatalogNodeId(1));

            byte[] data = new byte[0x10];
            Assert.Equal(13, buffer.Read(0, data, 0, data.Length));
        }

        /// <summary>
        /// <see cref="FileBuffer.Read(long, byte[], int, int)"/> works when data is split in
        /// two separate extents.
        /// </summary>
        [Fact]
        public void MultipleExtentRead_Works()
        {
            var buffer = new FileBuffer(
                new Context()
                {
                    VolumeStream =
                        SparseStream.FromStream(
                            new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!")), Ownership.Dispose),
                    VolumeHeader = new VolumeHeader()
                    {
                        BlockSize = 8,
                    },
                },
                new ForkData()
                {
                    LogicalSize = 0x123,
                    Extents = new ExtentDescriptor[]
                    {
                         new ExtentDescriptor()
                         {
                              BlockCount = 1,
                              StartBlock = 0,
                         },
                         new ExtentDescriptor()
                         {
                              BlockCount = 1,
                              StartBlock = 1,
                         },
                    },
                },
                new CatalogNodeId(1));

            byte[] data = new byte[0x10];
            Assert.Equal(8, buffer.Read(0, data, 0, data.Length));
            Assert.Equal(5, buffer.Read(8, data, 8, data.Length));
        }

        /// <summary>
        /// <see cref="FileBuffer.Read(long, byte[], int, int)"/> works when data is split in
        /// two separate overflow extents.
        /// </summary>
        [Fact]
        public void ReadFromOverflow_Works()
        {
            var cnid = new CatalogNodeId(1);

            var descriptor = new ExtentDescriptor() { BlockCount = 1, StartBlock = 0 };
            byte[] descriptorBytes = new byte[2 * descriptor.Size];
            descriptor.WriteTo(descriptorBytes, 0);

            descriptor.StartBlock = 1;
            descriptor.WriteTo(descriptorBytes, descriptor.Size);

            var extentsOverflow = new Mock<BTree<ExtentKey>>(MockBehavior.Strict);
            extentsOverflow
                .Setup(e => e.Find(new ExtentKey(cnid, 0, false)))
                .Returns(descriptorBytes);

            var buffer = new FileBuffer(
                new Context()
                {
                    VolumeStream =
                        SparseStream.FromStream(
                            new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!")), Ownership.Dispose),
                    VolumeHeader = new VolumeHeader()
                    {
                        BlockSize = 8,
                    },
                    ExtentsOverflow = extentsOverflow.Object,
                },
                new ForkData()
                {
                    LogicalSize = 0x123,
                    Extents = Array.Empty<ExtentDescriptor>(),
                    TotalBlocks = 2,
                },
                cnid);

            byte[] data = new byte[0x10];
            Assert.Equal(8, buffer.Read(0, data, 0, data.Length));
            Assert.Equal(5, buffer.Read(8, data, 8, data.Length));
        }

        /// <summary>
        /// <see cref="FileBuffer.Read(long, byte[], int, int)"/> throws when an extent is missing.
        /// </summary>
        [Fact]
        public void ReadFrom_MissingExtent_Throws()
        {
            var cnid = new CatalogNodeId(1);

            var extentsOverflow = new Mock<BTree<ExtentKey>>(MockBehavior.Strict);
            extentsOverflow
                .Setup(e => e.Find(new ExtentKey(cnid, 0, false)))
                .Returns((byte[])null);

            var buffer = new FileBuffer(
                new Context()
                {
                    VolumeStream = Stream.Null,
                    VolumeHeader = new VolumeHeader()
                    {
                        BlockSize = 8,
                    },
                    ExtentsOverflow = extentsOverflow.Object,
                },
                new ForkData()
                {
                    LogicalSize = 0x123,
                    Extents = Array.Empty<ExtentDescriptor>(),
                    TotalBlocks = 2,
                },
                cnid);

            byte[] data = new byte[0x10];
            Assert.Throws<IOException>(() => buffer.Read(0, data, 0, data.Length));
        }

        /// <summary>
        /// <see cref="FileBuffer.Read(long, byte[], int, int)"/> throws when an extent could not be located.
        /// </summary>
        [Fact]
        public void ReadFrom_BeyondEndOfFile_Throws()
        {
            var cnid = new CatalogNodeId(1);

            var extentsOverflow = new Mock<BTree<ExtentKey>>(MockBehavior.Strict);
            extentsOverflow
                .Setup(e => e.Find(new ExtentKey(cnid, 0, false)))
                .Returns((byte[])null);

            var buffer = new FileBuffer(
                new Context()
                {
                    VolumeStream = Stream.Null,
                    VolumeHeader = new VolumeHeader()
                    {
                        BlockSize = 8,
                    },
                    ExtentsOverflow = extentsOverflow.Object,
                },
                new ForkData()
                {
                    LogicalSize = 0x123,
                    Extents = Array.Empty<ExtentDescriptor>(),
                    TotalBlocks = 0,
                },
                cnid);

            byte[] data = new byte[0x10];
            Assert.Throws<InvalidOperationException>(() => buffer.Read(0, data, 0, data.Length));
        }
    }
}
