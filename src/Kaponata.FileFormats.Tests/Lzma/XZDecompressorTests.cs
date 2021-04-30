// <copyright file="XZDecompressorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Lzma;
using System;
using System.Buffers;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Lzma
{
    /// <summary>
    /// Tests the <see cref="XZDecompressor"/> class.
    /// </summary>
    public class XZDecompressorTests
    {
        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> returns <see cref="OperationStatus.NeedMoreData"/>
        /// when passed empty arrays.
        /// </summary>
        [Fact]
        public void NoData_ReturnsNeedMoreData()
        {
            using (XZDecompressor decompressor = new XZDecompressor())
            {
                byte[] source = Array.Empty<byte>();
                byte[] destination = Array.Empty<byte>();

                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out int bytesConsumed, out int bytesWritten));
                Assert.Equal(0, bytesConsumed);
                Assert.Equal(0, bytesWritten);
            }
        }

        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> returns <see cref="OperationStatus.NeedMoreData"/>
        /// when the output buffer is too small.
        /// </summary>
        [Fact]
        public void OutputToSmall_ReturnsError()
        {
            using (XZDecompressor decompressor = new XZDecompressor())
            {
                Span<byte> source = File.ReadAllBytes("Lzma/hello.xz");
                Span<byte> destination = Array.Empty<byte>();

                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out int bytesConsumed, out int bytesWritten));
                Assert.Equal(0x19, bytesConsumed);
                Assert.Equal(0, bytesWritten);

                source = source.Slice(bytesConsumed);

                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out bytesConsumed, out bytesWritten));
                Assert.Equal(2, bytesConsumed);
                Assert.Equal(0, bytesWritten);

                source = source.Slice(bytesConsumed);
                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out bytesConsumed, out bytesWritten));
                Assert.Equal(0, bytesConsumed);
                Assert.Equal(0, bytesWritten);

                destination = new byte[0x100];
                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out bytesConsumed, out bytesWritten));
                Assert.Equal(source.Length, bytesConsumed);
                Assert.Equal(0xe, bytesWritten);

                source = source.Slice(bytesConsumed);
                Assert.Equal(OperationStatus.Done, decompressor.Decompress(source, destination, out bytesConsumed, out bytesWritten));
                Assert.Equal(0, bytesConsumed);
                Assert.Equal(0, bytesWritten);
            }
        }

        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> returns <see cref="OperationStatus.InvalidData"/>
        /// when presented with invalid data.
        /// </summary>
        [Fact]
        public void InvalidData_ReturnsError()
        {
            using (XZDecompressor decompressor = new XZDecompressor())
            {
                Span<byte> source = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                Span<byte> destination = new byte[0x10];

                Assert.Equal(OperationStatus.InvalidData, decompressor.Decompress(source, destination, out int bytesConsumed, out int bytesWritten));
                Assert.Equal(0x4, bytesConsumed);
                Assert.Equal(0, bytesWritten);
            }
        }

        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> correctly decodes trivial data.
        /// </summary>
        /// <param name="path">
        /// The path to the file to decode.
        /// </param>
        [Theory]
        [InlineData("Lzma/hello.xz")]
        [InlineData("Lzma/hello.lzma")]
        public unsafe void Decode_Works(string path)
        {
            using (XZDecompressor decompressor = new XZDecompressor())
            {
                Span<byte> source = File.ReadAllBytes(path);
                Span<byte> destination = new byte[100];

                Assert.Equal(OperationStatus.NeedMoreData, decompressor.Decompress(source, destination, out int bytesConsumed, out int bytesWritten));
                Assert.Equal(source.Length, bytesConsumed);
                Assert.Equal(0xe, bytesWritten);

                Assert.Equal(OperationStatus.Done, decompressor.Decompress(Array.Empty<byte>(), destination, out bytesConsumed, out bytesWritten));
                Assert.Equal(0, bytesConsumed);
                Assert.Equal(0, bytesWritten);
            }
        }

        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> throws when parsing invalid data.
        /// </summary>
        /// <param name="path">
        /// The path to the file to parse.
        /// </param>
        /// <param name="format">
        /// The format to parse.
        /// </param>
        [Theory]
        [InlineData("Lzma/hello.lzma", LzmaFormat.Xz)]
        [InlineData("Lzma/hello.xz", LzmaFormat.Lzma)]
        public void Read_InvalidFormat_ReturnsError(string path, LzmaFormat format)
        {
            byte[] data = File.ReadAllBytes(path);
            byte[] output = new byte[100];

            using (XZDecompressor decompressor = new XZDecompressor(format))
            {
                Assert.Equal(OperationStatus.InvalidData, decompressor.Decompress(data, output, out int bytesConsumed, out int bytesWritten));
            }
        }

        /// <summary>
        /// <see cref="XZDecompressor.Dispose"/> can be invoked twice without errors.
        /// </summary>
        [Fact]
        public void Dispose_Idempotent()
        {
            XZDecompressor decompressor = new XZDecompressor();
            decompressor.Dispose();

            // .Dispose() can be invoked twice without errors
            decompressor.Dispose();
        }

        /// <summary>
        /// <see cref="XZDecompressor.Decompress(ReadOnlySpan{byte}, Span{byte}, out int, out int)"/> throws when the object has been disposed of.
        /// </summary>
        [Fact]
        public void Decompress_ThrowsWhenDisposed()
        {
            XZDecompressor decompressor = new XZDecompressor();
            decompressor.Dispose();

            Assert.Throws<ObjectDisposedException>(() => decompressor.Decompress(Array.Empty<byte>(), Array.Empty<byte>(), out _, out _));
        }
    }
}
