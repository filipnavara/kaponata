// <copyright file="H264DecoderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="H264Decoder"/> class.
    /// </summary>
    public class H264DecoderTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="H264DecoderTests"/> class.
        /// </summary>
        public H264DecoderTests()
        {
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="FrameBuffer.CopyFramebuffer(Memory{byte})"/> throws when the buffer is too small.
        /// </summary>
        [Fact]
        public void CopyFrameBuffer_ThrowsOnWrongSize()
        {
            using (var stream = File.OpenRead("video.h264"))
            using (var decoder = new H264Decoder(stream))
            {
                var frameBuffer = decoder.FrameBuffer;
                frameBuffer.FrameReceived += (sender, e) =>
                {
                    byte[] array = new byte[20];
                    Assert.Throws<ArgumentOutOfRangeException>(() => frameBuffer.CopyFramebuffer(array));
                };

                decoder.Decode();
            }
        }

        /// <summary>
        /// Tests the <see cref="H264Decoder.Decode"/> method.
        /// </summary>
        [Fact]
        public unsafe void DecodeTest()
        {
            int frameCount = 0;

            var imageHashes = new List<string>();

            using (var stream = File.OpenRead("video.h264"))
            using (var decoder = new H264Decoder(stream))
            {
                var frameBuffer = decoder.FrameBuffer;
                frameBuffer.FrameReceived += (sender, e) =>
                {
                    Assert.Equal(1334, frameBuffer.Height);
                    Assert.Equal(3000, frameBuffer.Stride);
                    Assert.Equal(750, frameBuffer.Width);
                    Assert.Equal(750, frameBuffer.AlignedWidth);
                    Assert.Equal(1334, frameBuffer.AlignedHeight);

                    byte[] array = new byte[frameBuffer.FrameBufferSize];
                    frameBuffer.CopyFramebuffer(array);

                    fixed (byte* ptr = array)
                    {
                        // Only reason we do this is to make sure the framebuffer data is relevant
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            using (var bitmap = new Bitmap(frameBuffer.Width, frameBuffer.Height, frameBuffer.Stride, PixelFormat.Format24bppRgb, (IntPtr)ptr))
                            {
                                bitmap.Save($"stream-frame.{frameCount}.jpg", ImageFormat.Jpeg);
                            }
                        }
                    }

                    frameCount++;
                };

                decoder.Decode();
            }

            Assert.Equal(5, frameCount);
        }

        /// <summary>
        /// Tests the <see cref="H264Decoder.Start"/> method, and lets the decoding run until
        /// all data is consumed.
        /// </summary>
        /// <returns>
        /// At <see cref="Task"/> representing the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Start_Completes_Async()
        {
            int frameCount = 0;

            Collection<string> imageHashes = new Collection<string>();

            using (var stream = File.OpenRead("video.h264"))
            using (var decoder = new H264Decoder(stream))
            {
                var frameBuffer = decoder.FrameBuffer;
                frameBuffer.FrameReceived += (sender, e) =>
                {
                    Assert.Equal(1334, frameBuffer.Height);
                    Assert.Equal(3000, frameBuffer.Stride);
                    Assert.Equal(750, frameBuffer.Width);
                    Assert.Equal(750, frameBuffer.AlignedWidth);
                    Assert.Equal(1334, frameBuffer.AlignedHeight);

                    frameCount++;
                };

                decoder.Start();
                await Task.WhenAny(
                    decoder.DecodeTask,
                    Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);

                Assert.True(decoder.DecodeTask.IsCompleted, "The decode task did not complete in time");
            }

            Assert.Equal(5, frameCount);
        }

        /// <summary>
        /// Tests common methods on a disposed instance of the <see cref="H264Decoder"/> class.
        /// </summary>
        [Fact]
        public void Disposed_ThrowsException()
        {
            var decoder = new H264Decoder(Stream.Null);
            decoder.Dispose();

            Assert.Throws<ObjectDisposedException>(() => decoder.Start());
            Assert.Throws<ObjectDisposedException>(() => decoder.FrameBuffer.CopyFramebuffer(Array.Empty<byte>()));
            Assert.Throws<ObjectDisposedException>(() => decoder.Decode());
        }

        /// <summary>
        /// The <see cref="H264Decoder.Dispose"/> method returns if the decoder was disposed previously.
        /// </summary>
        [Fact]
        public void Dispose_Twice()
        {
            using var stream = File.OpenRead("video.h264");

            var decoder = new H264Decoder(stream);
            decoder.Dispose();
            decoder.Dispose();
        }

        /// <summary>
        /// The <see cref="H264Decoder.Start"/> method throws if the decode task is already completed.
        /// </summary>
        [Fact]
        public void Start_IsCompleted()
        {
            using var stream = File.OpenRead("video.h264");
            using var decoder = new H264Decoder(stream);

            decoder.Start();
            Assert.Throws<InvalidOperationException>(() => decoder.Start());
        }
    }
}
