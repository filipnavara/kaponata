// <copyright file="AVCodecHandleTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using System;
using Xunit;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVCodecHandle"/> class.
    /// </summary>
    public unsafe class AVCodecHandleTests
    {
        /// <summary>
        /// The <see cref="AVCodecHandle.AVCodecHandle(NativeAVCodec*)"/> initializes the  instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesInstance()
        {
            NativeAVCodec codec = new NativeAVCodec
            { };

            var handle = new AVCodecHandle(&codec);
            Assert.False(handle.IsInvalid);
            Assert.Equal(new IntPtr(&codec), handle.DangerousGetHandle());

            handle = new AVCodecHandle(default);
            Assert.True(handle.IsInvalid);
        }
    }
}
