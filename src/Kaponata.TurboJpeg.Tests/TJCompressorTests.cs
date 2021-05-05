// <copyright file="TJCompressorTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace Kaponata.TurboJpeg.Tests
{
    /// <summary>
    /// Tests the <see cref="TJCompressor"/> class.
    /// </summary>
    public class TJCompressorTests : IDisposable
    {
        private TJCompressor compressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJCompressorTests"/> class.
        /// </summary>
        public TJCompressorTests()
        {
            this.compressor = new TJCompressor();
            if (Directory.Exists(this.OutDirectory))
            {
                Directory.Delete(this.OutDirectory, true);
            }

            Directory.CreateDirectory(this.OutDirectory);
        }

        private string OutDirectory
        {
            get { return Path.Combine(TestUtils.BinPath, "compress_images_out"); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.compressor.Dispose();
        }

        /// <summary>
        /// Tests the <see cref="TJCompressor.Compress(Span{byte}, Span{byte}, int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> method.
        /// </summary>
        /// <param name="options">
        /// Options to pass to the compressor.
        /// </param>
        /// <param name="quality">
        /// The compression quality to use.
        /// </param>
        [Theory]
        [CombinatorialData]
        public void CompressByteArrayToByteArray(
            [CombinatorialValues(
            TJSubsamplingOption.Gray,
            TJSubsamplingOption.Chrominance411,
            TJSubsamplingOption.Chrominance420,
            TJSubsamplingOption.Chrominance440,
            TJSubsamplingOption.Chrominance422,
            TJSubsamplingOption.Chrominance444)]
            TJSubsamplingOption options,
            [CombinatorialValues(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]
            int quality)
        {
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    var data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    var stride = data.Stride;
                    var width = data.Width;
                    var height = data.Height;
                    var pixelFormat = data.PixelFormat;
                    Assert.Equal(PixelFormat.Format24bppRgb, pixelFormat);

                    var buf = new byte[stride * height];
                    Marshal.Copy(data.Scan0, buf, 0, buf.Length);
                    bitmap.UnlockBits(data);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    byte[] target = new byte[this.compressor.GetBufferSize(width, height, options)];

                    this.compressor.Compress(buf, target, stride, width, height, TJPixelFormat.RGB, options, quality, TJFlags.None);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// Tests the <see cref="TJCompressor.Compress(Span{byte}, Span{byte}, int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> method.
        /// </summary>
        /// <param name="options">
        /// Options to pass to the compressor.
        /// </param>
        /// <param name="quality">
        /// The compression quality to use.
        /// </param>
        [Theory]
        [CombinatorialData]
        public unsafe void CompressSpanToSpan(
            [CombinatorialValues(
            TJSubsamplingOption.Gray,
            TJSubsamplingOption.Chrominance411,
            TJSubsamplingOption.Chrominance420,
            TJSubsamplingOption.Chrominance440,
            TJSubsamplingOption.Chrominance422,
            TJSubsamplingOption.Chrominance444)]
            TJSubsamplingOption options,
            [CombinatorialValues(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]
            int quality)
        {
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    var data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    var stride = data.Stride;
                    var width = data.Width;
                    var height = data.Height;
                    var pixelFormat = data.PixelFormat;
                    Assert.Equal(PixelFormat.Format24bppRgb, pixelFormat);

                    Span<byte> buf = new Span<byte>((byte*)data.Scan0, stride * height);
                    bitmap.UnlockBits(data);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    Span<byte> target = new byte[this.compressor.GetBufferSize(width, height, options)];

                    this.compressor.Compress(buf, target, stride, width, height, TJPixelFormat.RGB, options, quality, TJFlags.None);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// <see cref="TJCompressor.Compress(Span{byte}, Span{byte}, int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> throws on errors.
        /// </summary>
        [Fact]
        public void Compress_ThrowsOnError()
        {
            Assert.Throws<TJException>(() => this.compressor.Compress(Span<byte>.Empty, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.RGB, TJSubsamplingOption.Gray, 0, TJFlags.None));
            Assert.Throws<NotSupportedException>(() => this.compressor.Compress(Span<byte>.Empty, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.Gray, TJSubsamplingOption.Chrominance444, 0, TJFlags.None));
        }

        /// <summary>
        /// <see cref="TJCompressor"/> methods throw when disposed.
        /// </summary>
        [Fact]
        public void Methods_ThrowWhenDisposed()
        {
            this.compressor.Dispose();

            Assert.True(this.compressor.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() => this.compressor.Compress(Span<byte>.Empty, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.RGB, TJSubsamplingOption.Gray, 0, TJFlags.None));

            Assert.Throws<ObjectDisposedException>(() => this.compressor.CompressFromYUVPlanes(Span<byte>.Empty, Span<byte>.Empty, Span<byte>.Empty, 0, Array.Empty<int>(), 0, TJSubsamplingOption.Gray, Span<byte>.Empty, 0, TJFlags.None));
            Assert.Throws<ObjectDisposedException>(() => this.compressor.GetBufferSize(0, 0, TJSubsamplingOption.Gray));
        }

        /// <summary>
        /// <see cref="TJCompressor.PlaneWidth(TJPlane, int, int)"/> returns correct values.
        /// </summary>
        [Fact]
        public void PlaneWidth_Works()
        {
            Assert.Equal(10, this.compressor.PlaneWidth(TJPlane.U, 10, 4));
        }

        /// <summary>
        /// <see cref="TJCompressor.PlaneHeight(TJPlane, int, int)"/> returns correct values.
        /// </summary>
        [Fact]
        public void PlaneHeight_Works()
        {
            Assert.Equal(5, this.compressor.PlaneHeight(TJPlane.U, 10, 4));
        }

        /// <summary>
        /// <see cref="TJCompressor.CompressFromYUVPlanes(Span{byte}, Span{byte}, Span{byte}, int, int[], int, TJSubsamplingOption, Span{byte}, int, TJFlags)"/> can correctly handle
        /// a color image.
        /// </summary>
        [Fact]
        public void CompressFromYUVPlanes_ColorImage()
        {
            const int width = 2;
            const int height = 2;

            byte[] yPlane = new byte[this.compressor.PlaneSizeYUV(TJPlane.Y, width, stride: 0, height, 4)];
            byte[] uPlane = new byte[this.compressor.PlaneSizeYUV(TJPlane.U, width, stride: 0, height, 4)];
            byte[] vPlane = new byte[this.compressor.PlaneSizeYUV(TJPlane.V, width, stride: 0, height, 4)];

            var result = this.compressor.CompressFromYUVPlanes(yPlane, uPlane, vPlane, width, new int[] { 0, 0, 0 }, height, TJSubsamplingOption.Chrominance444, null, 100, TJFlags.None);

            // Compressed images starts with the JPEG magic.
            Assert.Equal(0xffd8ffe0, BinaryPrimitives.ReadUInt32BigEndian(result.Slice(0, 4)));
        }

        /// <summary>
        /// <see cref="TJCompressor.CompressFromYUVPlanes(Span{byte}, Span{byte}, Span{byte}, int, int[], int, TJSubsamplingOption, Span{byte}, int, TJFlags)"/> can correctly handle
        /// a grayscale image.
        /// </summary>
        [Fact]
        public void CompressFromYUVPlanes_GrayImage()
        {
            const int width = 2;
            const int height = 2;

            byte[] yPlane = new byte[this.compressor.PlaneSizeYUV(TJPlane.Y, width, stride: 0, height, 4)];

            var result = this.compressor.CompressFromYUVPlanes(yPlane, null, null, width, new int[] { 0, }, height, TJSubsamplingOption.Gray, null, 100, TJFlags.None);

            // Compressed images starts with the JPEG magic.
            Assert.Equal(0xffd8ffe0, BinaryPrimitives.ReadUInt32BigEndian(result.Slice(0, 4)));
        }
    }
}
