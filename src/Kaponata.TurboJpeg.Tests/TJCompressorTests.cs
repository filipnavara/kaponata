// <copyright file="TJCompressorTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
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
        /// Tests the <see cref="TJCompressor.Compress(IntPtr, int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> method.
        /// </summary>
        /// <param name="options">
        /// Options to pass to the compressor.
        /// </param>
        /// <param name="quality">
        /// The compression quality to use.
        /// </param>
        [Theory]
        [CombinatorialData]
        public void CompressIntPtr(
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
                BitmapData data = null;
                try
                {
                    data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    Assert.Equal(PixelFormat.Format24bppRgb, bitmap.PixelFormat);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    var result = this.compressor.Compress(data.Scan0, data.Stride, data.Width, data.Height, TJPixelFormat.RGB, options, quality, TJFlags.None);
                    Assert.NotNull(result);
                }
                finally
                {
                    if (data != null)
                    {
                        bitmap.UnlockBits(data);
                    }

                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// <see cref="TJCompressor.Compress(IntPtr, int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> throws when passed an invalid <see cref="IntPtr"/> value.
        /// </summary>
        [Fact]
        public void CompressInvalidIntPtr()
        {
            Assert.Throws<TJException>(() => this.compressor.Compress(IntPtr.Zero, 0, 0, 0, TJPixelFormat.ARGB, TJSubsamplingOption.Gray, 0, TJFlags.None));
        }

        /// <summary>
        /// Tests the <see cref="TJCompressor.Compress(byte[], int, int, int, TJPixelFormat, TJSubsamplingOption, int, TJFlags)"/> method.
        /// </summary>
        /// <param name="options">
        /// Options to pass to the compressor.
        /// </param>
        /// <param name="quality">
        /// The compression quality to use.
        /// </param>
        [Theory]
        [CombinatorialData]
        public void CompressByteArray(
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
                    var result = this.compressor.Compress(buf, stride, width, height, TJPixelFormat.RGB, options, quality, TJFlags.None);
                    Assert.NotNull(result);
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
        /// <see cref="TJCompressor"/> methods throw when disposed.
        /// </summary>
        [Fact]
        public void Methods_ThrowWhenDisposed()
        {
            this.compressor.Dispose();

            Assert.True(this.compressor.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() => this.compressor.Compress(Array.Empty<byte>(), 0, 0, 0, TJPixelFormat.RGB, TJSubsamplingOption.Gray, 0, TJFlags.None));
            Assert.Throws<ObjectDisposedException>(() => this.compressor.Compress(IntPtr.Zero, 0, 0, 0, TJPixelFormat.RGB, TJSubsamplingOption.Gray, 0, TJFlags.None));
            Assert.Throws<ObjectDisposedException>(() => this.compressor.Compress(Span<byte>.Empty, Span<byte>.Empty, 0, 0, 0, TJPixelFormat.RGB, TJSubsamplingOption.Gray, 0, TJFlags.None));

            Assert.Throws<ObjectDisposedException>(() => this.compressor.CompressFromYUVPlanes(Span<byte>.Empty, Span<byte>.Empty, Span<byte>.Empty, 0, Array.Empty<int>(), 0, TJSubsamplingOption.Gray, Span<byte>.Empty, 0, TJFlags.None));
            Assert.Throws<ObjectDisposedException>(() => this.compressor.GetBufferSize(0, 0, TJSubsamplingOption.Gray));
        }
    }
}
