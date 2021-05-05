// <copyright file="TJDecompressorTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Drawing.Imaging;
using System.IO;
using Xunit;

namespace TurboJpegWrapper.Tests
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
        /// <see cref="TJDecompressor.Decompress(byte[], TJPixelFormat, TJFlags, out int, out int, out int)"/> works correctly.
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
    }
}
