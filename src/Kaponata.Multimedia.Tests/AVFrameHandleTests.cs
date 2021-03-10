// <copyright file="AVFrameHandleTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Moq;
using System;
using Xunit;
using NativeAVFrame = FFmpeg.AutoGen.AVFrame;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVFrameHandle"/> class.
    /// </summary>
    public unsafe class AVFrameHandleTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVFrameHandleTests"/> class.
        /// </summary>
        public AVFrameHandleTests()
        {
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVFrameHandle.AVFrameHandle(FFmpegClient)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeFrame(new IntPtr(1245)))
                .Verifiable();
            ffmpegMock
                .Setup(c => c.AllocFrame())
                .Returns(new IntPtr(1245))
                .Verifiable();

            using (var handle = new AVFrameHandle(ffmpegMock.Object))
            {
                Assert.Equal(1245, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFrameHandle.AVFrameHandle(FFmpegClient, NativeAVFrame*)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constuctor_GivenNativeFrame_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            NativeAVFrame frame = new NativeAVFrame()
            {
                height = 12354,
            };

            ffmpegMock
                .Setup(c => c.FreeFrame(It.IsAny<IntPtr>()))
                .Verifiable();

            using (var handle = new AVFrameHandle(ffmpegMock.Object, &frame))
            {
                Assert.Equal((int)&frame, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }
    }
}
