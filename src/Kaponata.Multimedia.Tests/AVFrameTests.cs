// <copyright file="AVFrameTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Moq;
using System;
using System.Linq;
using Xunit;
using NativeAVFrame = FFmpeg.AutoGen.AVFrame;
using NativeAVPictureType = FFmpeg.AutoGen.AVPictureType;
using NativeAVPixelFormat = FFmpeg.AutoGen.AVPixelFormat;
using NativeIntArray = FFmpeg.AutoGen.int_array8;
using NativePointerArray = FFmpeg.AutoGen.byte_ptrArray8;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVFrame"/> class.
    /// </summary>
    public unsafe class AVFrameTests
    {
        /// <summary>
        /// Tests the <see cref="AVFrame.AVFrame(FFmpegClient)"/> constructor.
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

            using (var frame = new AVFrame(ffmpegMock.Object))
            {
                Assert.Equal(1245, (int)frame.Handle.DangerousGetHandle().ToPointer());
                Assert.Equal(1245, (int)frame.NativeObject);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// Verifies whether the <see cref="AVFrame"/> properies return the native values.
        /// </summary>
        [Fact]
        public void Properties_ReturnNativeValues()
        {
            var presentationTimestamp = 4525;
            var width = 5465415;
            var height = 654312;
            var lineSize = default(NativeIntArray);
            var lineSizeValues = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            lineSize.UpdateFrom(lineSizeValues);
            var data = default(NativePointerArray);
            var dataValues = new byte*[] { (byte*)8, (byte*)7, (byte*)6, (byte*)5, (byte*)4, (byte*)3, (byte*)2, (byte*)1 };
            data.UpdateFrom(dataValues);
            var pictureType = NativeAVPictureType.AV_PICTURE_TYPE_P;
            var pixelFormat = NativeAVPixelFormat.AV_PIX_FMT_AYUV64BE;

            NativeAVFrame nativeFrame = new NativeAVFrame()
            {
                pts = presentationTimestamp,
                width = width,
                height = height,
                linesize = lineSize,
                data = data,
                pict_type = pictureType,
                format = (int)pixelFormat,
            };

            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeFrame(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpegClient = ffmpegMock.Object;

            using (var frame = new AVFrame(ffmpegClient, new AVFrameHandle(ffmpegClient, &nativeFrame)))
            {
                Assert.Equal(presentationTimestamp, frame.PresentationTimestamp);
                Assert.Equal(width, frame.Width);
                Assert.Equal(height, frame.Height);
                Assert.Equal(lineSizeValues, frame.LineSize.ToArray());
                Assert.Equal(8, (int)frame.Data[0]);
                Assert.Equal(7, (int)frame.Data[1]);
                Assert.Equal(6, (int)frame.Data[2]);
                Assert.Equal(5, (int)frame.Data[3]);
                Assert.Equal(4, (int)frame.Data[4]);
                Assert.Equal(3, (int)frame.Data[5]);
                Assert.Equal(2, (int)frame.Data[6]);
                Assert.Equal(1, (int)frame.Data[7]);
                Assert.Equal(pictureType, frame.PictureType);
                Assert.Equal(pixelFormat, frame.Format);
            }

            ffmpegMock.Verify();
        }
    }
}
