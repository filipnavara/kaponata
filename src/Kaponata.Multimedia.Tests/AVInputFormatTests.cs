// <copyright file="AVInputFormatTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Moq;
using System;
using Xunit;
using NativeAVInputFormat = FFmpeg.AutoGen.AVInputFormat;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVInputFormat"/> class.
    /// </summary>
    public unsafe class AVInputFormatTests
    {
        /// <summary>
        /// Tests the <see cref="AVInputFormat.AVInputFormat(FFmpegClient, string)"/> constructor.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            var name = new byte[] { (byte)'h', (byte)'2', (byte)'6', (byte)'4' };
            var longName = new byte[] { (byte)'h', (byte)'2', (byte)'6', (byte)'4', (byte)'_', (byte)'l', (byte)'o', (byte)'n', (byte)'g' };
            fixed (byte* namePtr = name)
            fixed (byte* longNamePtr = longName)
            {
                var nativeInputFormat = new NativeAVInputFormat()
                {
                    name = namePtr,
                    long_name = longNamePtr,
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                ffmpegMock
                    .Setup(c => c.FindInputFormat("h264"))
                    .Returns((IntPtr)(&nativeInputFormat));
                var ffmpegClient = ffmpegMock.Object;

                var inputFormat = new AVInputFormat(ffmpegClient, "h264");

                Assert.Equal("h264", inputFormat.Name);
                Assert.Equal("h264_long", inputFormat.LongName);
                Assert.Equal("h264_long", inputFormat.ToString());
                Assert.Equal((int)&nativeInputFormat, (int)inputFormat.NativeObject);
            }
        }
    }
}
