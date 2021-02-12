// <copyright file="AVFormatContextHandleTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Moq;
using System;
using Xunit;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVFormatContextHandle"/> class.
    /// </summary>
    public unsafe class AVFormatContextHandleTests
    {
        /// <summary>
        /// The <see cref="AVFormatContextHandle.AVFormatContextHandle(FFMpegClient, FFmpeg.AutoGen.AVFormatContext*)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                duration = 10,
            };

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();

            using (var handle = new AVFormatContextHandle(ffmpegMock.Object, &nativeAVFormatContext))
            {
                Assert.Equal((int)&nativeAVFormatContext, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }
    }
}
