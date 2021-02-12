// <copyright file="AVIOContextHandleTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Moq;
using System.Runtime.InteropServices;
using Xunit;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVIOContextHandle"/> class.
    /// </summary>
    public unsafe class AVIOContextHandleTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContextHandleTests"/> class.
        /// </summary>
        public AVIOContextHandleTests()
        {
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVIOContextHandle.AVIOContextHandle(FFMpegClient, NativeAVIOContext*, bool)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void ConstuctorOwnsHandle_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var nativeIOContext = new NativeAVIOContext
            {
                pos = 5,
            };

            ffmpegMock
                .Setup(c => c.FreeAVHandle(It.IsAny<SafeHandle>()))
                .Verifiable();

            using (var handle = new AVIOContextHandle(ffmpegMock.Object, &nativeIOContext, true))
            {
                Assert.Equal((int)&nativeIOContext, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVIOContextHandle.AVIOContextHandle(FFMpegClient, FFmpeg.AutoGen.AVIOContext*)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var nativeIOContext = new NativeAVIOContext
            {
                pos = 5,
            };

            ffmpegMock
                .Setup(c => c.FreeAVHandle(It.IsAny<SafeHandle>()))
                .Verifiable();

            using (var handle = new AVIOContextHandle(ffmpegMock.Object, &nativeIOContext))
            {
                Assert.Equal((int)&nativeIOContext, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }
    }
}
