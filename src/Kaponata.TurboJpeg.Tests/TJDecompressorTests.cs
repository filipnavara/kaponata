// <copyright file="TJDecompressorTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.IO;
using Xunit;

namespace Kaponata.TurboJpeg.Tests
{
    /// <summary>
    /// Tests the <see cref="TJDecompressor"/> class.
    /// </summary>
    public class TJDecompressorTests : IDisposable
    {
        private TJDecompressor decompressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJDecompressorTests"/> class.
        /// </summary>
        public TJDecompressorTests()
        {
            this.decompressor = new TJDecompressor();
            if (Directory.Exists(this.OutDirectory))
            {
                Directory.Delete(this.OutDirectory, true);
            }

            Directory.CreateDirectory(this.OutDirectory);
        }

        private string OutDirectory
        {
            get { return Path.Combine(TestUtils.BinPath, "decompress_images_out"); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.decompressor.Dispose();
        }

        /// <summary>
        /// <see cref="TJDecompressor.Decompress(Span{byte}, Span{byte}, TJPixelFormat, TJFlags, out int, out int, out int)"/> works correctly.
        /// </summary>
        /// <param name="format">
        /// The format of the destination image.
        /// </param>
        [Theory]
        [CombinatorialData]
        public void DecompressSpan(
            [CombinatorialValues(
            TJPixelFormat.ABGR,
            TJPixelFormat.RGB,
            TJPixelFormat.Gray)]
            TJPixelFormat format)
        {
            byte[] outBuf = ArrayPool<byte>.Shared.Rent(250 * 250 * 4);

            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var dataSpan = data.Item2.AsSpan();
                this.decompressor.Decompress(dataSpan, outBuf.AsSpan(), format, TJFlags.None, out int width, out int height, out int stride);
            }

            ArrayPool<byte>.Shared.Return(outBuf);
        }

        /// <summary>
        /// <see cref="TJDecompressor.Decompress(Span{byte}, Span{byte}, TJPixelFormat, TJFlags, out int, out int, out int)"/> works correctly.
        /// </summary>
        /// <param name="format">
        /// The format of the destination image.
        /// </param>
        [Theory]
        [CombinatorialData]
        public void DecompressArray(
            [CombinatorialValues(
            TJPixelFormat.ABGR,
            TJPixelFormat.RGB,
            TJPixelFormat.Gray)]
            TJPixelFormat format)
        {
            byte[] outBuf = ArrayPool<byte>.Shared.Rent(250 * 250 * 4);

            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                this.decompressor.Decompress(data.Item2, outBuf, format, TJFlags.None, out int width, out int height, out int stride);
            }

            ArrayPool<byte>.Shared.Return(outBuf);
        }

        /// <summary>
        /// <see cref="TJDecompressor.Decompress(Span{byte}, Span{byte}, TJPixelFormat, TJFlags, out int, out int, out int)"/> throws when an invalid argument is specified.
        /// </summary>
        [Fact]
        public void Decompress_InvalidArguments_ThrowsOnError()
        {
            Assert.Throws<TJException>(() => this.decompressor.Decompress(Span<byte>.Empty, Span<byte>.Empty, TJPixelFormat.Gray, TJFlags.None, out int _, out int _, out int _));
        }

        /// <summary>
        /// <see cref="TJDecompressor.Decompress(Span{byte}, Span{byte}, TJPixelFormat, TJFlags, out int, out int, out int)"/> throws when the output buffer is too smal.
        /// </summary>
        [Fact]
        public void Decompress_BufferTooSmall_ThrowsOnError()
        {
            byte[] source = File.ReadAllBytes("TestAssets/testorig.jpg");

            Assert.Throws<ArgumentOutOfRangeException>(() => this.decompressor.Decompress(source, Span<byte>.Empty, TJPixelFormat.Gray, TJFlags.None, out int _, out int _, out int _));
        }

        /// <summary>
        /// <see cref="TJDecompressor.Decompress(Span{byte}, Span{byte}, TJPixelFormat, TJFlags, out int, out int, out int)"/> throws when the JPEG image is truncated.
        /// </summary>
        [Fact]
        public void Decompress_Error_ThrowsOnError()
        {
            byte[] source = File.ReadAllBytes("TestAssets/testorig.jpg");

            using (var memoryOwner = MemoryPool<byte>.Shared.Rent(250 * 250 * 4))
            {
                // Premature end of file
                Assert.Throws<TJException>(() => this.decompressor.Decompress(source.AsSpan(0, source.Length - 1), memoryOwner.Memory.Span, TJPixelFormat.ABGR, TJFlags.None, out int _, out int _, out int _));
            }
        }

        /// <summary>
        /// <see cref="TJDecompressor.DecodeYUVPlanes(Span{byte}, Span{byte}, Span{byte}, int[], TJSubsamplingOption, Span{byte}, int, int, int, TJPixelFormat, TJFlags)"/> throws
        /// when invalid arguments are passed.
        /// </summary>
        [Fact]
        public void DecodeYUVPlanes_ThrowsOnError()
        {
            Assert.Throws<TJException>(() => this.decompressor.DecodeYUVPlanes(Span<byte>.Empty, Span<byte>.Empty, Span<byte>.Empty, new int[] { 0, 0, 0 }, TJSubsamplingOption.Chrominance444, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.CMYK, TJFlags.None));
        }

        /// <summary>
        /// <see cref="TJDecompressor.DecodeYUVPlanes(Span{byte}, Span{byte}, Span{byte}, int[], TJSubsamplingOption, Span{byte}, int, int, int, TJPixelFormat, TJFlags)"/>
        /// can decode YUV planes.
        /// </summary>
        [Fact]
        public void DecodeYUVPlanes_Works()
        {
            var yPlane = new byte[100];
            var uPlane = new byte[100];
            var vPlane = new byte[100];

            byte[] destination = new byte[400];

            this.decompressor.DecodeYUVPlanes(yPlane, uPlane, vPlane, new int[] { 10, 10, 10 }, TJSubsamplingOption.Chrominance444, destination, 10, 10, 10, TJPixelFormat.RGB, TJFlags.None);
        }

        /// <summary>
        /// <see cref="TJDecompressor.DecodeYUVPlanes(Span{byte}, Span{byte}, Span{byte}, int[], TJSubsamplingOption, Span{byte}, int, int, int, TJPixelFormat, TJFlags)"/>
        /// can decode grayscale images.
        /// </summary>
        [Fact]
        public void DecodeGrayscale_Works()
        {
            var yPlane = new byte[100];

            byte[] destination = new byte[400];

            this.decompressor.DecodeYUVPlanes(yPlane, Span<byte>.Empty, Span<byte>.Empty, new int[] { 10, 0, 0 }, TJSubsamplingOption.Gray, destination, 10, 10, 10, TJPixelFormat.Gray, TJFlags.None);
        }

        /// <summary>
        /// <see cref="TJDecompressor.GetImageInfo(Span{byte}, TJPixelFormat, out int, out int, out int, out int)"/> works correctly.
        /// </summary>
        [Fact]
        public void GetImageInfo_Works()
        {
            byte[] image = File.ReadAllBytes("TestAssets/testorig.jpg");

            this.decompressor.GetImageInfo(image, TJPixelFormat.ABGR, out int width, out int height, out int stride, out int bufSize);

            Assert.Equal(0xe3, width);
            Assert.Equal(0x95, height);
            Assert.Equal(0x38c, stride);
            Assert.Equal(0x2107c, bufSize);
        }

        /// <summary>
        /// <see cref="TJDecompressor.GetBufferSize(int, int, TJPixelFormat)"/> returns the correct value.
        /// </summary>
        [Fact]
        public void GetBufferSize_Works()
        {
            Assert.Equal(400, this.decompressor.GetBufferSize(10, 10, TJPixelFormat.RGBA));
        }

        /// <summary>
        /// <see cref="TJDecompressor"/> methods throw when disposed.
        /// </summary>
        [Fact]
        public void Methods_ThrowWhenDisposed()
        {
            this.decompressor.Dispose();

            Assert.True(this.decompressor.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() => this.decompressor.DecodeYUVPlanes(Span<byte>.Empty, Span<byte>.Empty, Span<byte>.Empty, Array.Empty<int>(), TJSubsamplingOption.Gray, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.Gray, TJFlags.None));
            Assert.Throws<ObjectDisposedException>(() => this.decompressor.Decompress(Span<byte>.Empty, Span<byte>.Empty, TJPixelFormat.Gray, TJFlags.None, out int _, out int _, out int _));
            Assert.Throws<ObjectDisposedException>(() => this.decompressor.GetBufferSize(0, 0, TJPixelFormat.Gray));
            Assert.Throws<ObjectDisposedException>(() => this.decompressor.GetImageInfo(Span<byte>.Empty, TJPixelFormat.ABGR, out int _, out int _, out int _, out int _));
        }
    }
}
