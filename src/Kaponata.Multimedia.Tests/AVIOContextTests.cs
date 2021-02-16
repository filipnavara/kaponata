// <copyright file="AVIOContextTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVIOContext"/> class.
    /// </summary>
    public unsafe class AVIOContextTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContextTests"/> class.
        /// </summary>
        public AVIOContextTests()
        {
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVIOContext.AVIOContext(FFMpegClient, AVIOContextHandle)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var bytes = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };

            var bytesHandle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, bytesHandle, bytes.Length);

            var nativeIOContext = new NativeAVIOContext
            {
                buffer = (byte*)bytesHandle,
                pos = 5,
            };

            ffmpegMock
                .Setup(c => c.FreeAVHandle(It.IsAny<SafeHandle>()))
                .Verifiable();

            var ffmpeg = ffmpegMock.Object;
            using (var handle = new AVIOContextHandle(ffmpeg, &nativeIOContext))
            using (var context = new AVIOContext(ffmpeg, handle))
            {
                Assert.Equal((int)&nativeIOContext, (int)context.NativeObject);
                var result = new byte[4];
                Marshal.Copy((IntPtr)context.Buffer.NativeObject, result, 0, bytes.Length);
                Assert.Equal(bytes, result);
            }

            ffmpegMock.Verify();

            Marshal.FreeHGlobal(bytesHandle);
        }
    }
}
