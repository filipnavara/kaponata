// <copyright file="AVPacketTestcs.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Moq;
using System;
using Xunit;
using NativeAVPacket = FFmpeg.AutoGen.AVPacket;
using NativeFFmpeg = FFmpeg.AutoGen.ffmpeg;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVPacket"/> class.
    /// </summary>
    public unsafe class AVPacketTestcs
    {
        /// <summary>
        /// Tests the <see cref="AVPacket.AVPacket(FFmpegClient)"/> constructor.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            IntPtr createdHandle = default(IntPtr);
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.InitPacket(It.IsAny<IntPtr>()))
                .Callback<IntPtr>(p => createdHandle = p)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.UnrefPacket(It.IsAny<AVPacket>()))
                .Verifiable();

            var ffmpegClient = ffmpegMock.Object;

            using (var packet = new AVPacket(ffmpegClient))
            {
                Assert.Equal(createdHandle, packet.Handle);
                Assert.Equal((int)createdHandle.ToPointer(), (int)packet.NativeObject);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVPacket"/> parameters return the native values.
        /// </summary>
        [Fact]
        public void Parameters_ReturnNativeValues()
        {
            var data = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };

            var nativePacket = new NativeAVPacket()
            {
            };

            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.AllocPacket())
                .Returns((IntPtr)(&nativePacket));

            ffmpegMock
                .Setup(c => c.InitPacket(It.IsAny<IntPtr>()))
                .Callback<IntPtr>(p =>
                {
                    fixed (byte* dataPtr = data)
                    {
                        var handle = (NativeAVPacket*)p;
                        handle->stream_index = 10;
                        handle->size = 11;
                        handle->dts = 12;
                        handle->duration = 13;
                        handle->flags = (int)AVPacketFlags.Discard;
                        handle->pos = 14;
                        handle->pts = 15;
                        handle->data = dataPtr;
                    }
                })
                .Verifiable();

            ffmpegMock
                .Setup(c => c.UnrefPacket(It.IsAny<AVPacket>()))
                .Verifiable();

            var ffmpegClient = ffmpegMock.Object;

            using (var packet = new AVPacket(ffmpegClient))
            {
                Assert.Equal(10, packet.StreamIndex);
                Assert.Equal(11, packet.Size);
                Assert.Equal(12, packet.DecompressionTimestamp);
                Assert.Equal(13, packet.Duration);
                Assert.Equal(AVPacketFlags.Discard, packet.Flags);
                Assert.Equal(14, packet.Position);
                Assert.Equal(15, packet.PresentationTimestamp);
                Assert.Equal((byte)'t', *packet.Data);
                Assert.Equal((byte)'e', *(packet.Data + 1));
                Assert.Equal((byte)'s', *(packet.Data + 2));
                Assert.Equal((byte)'t', *(packet.Data + 3));
            }
        }

        /// <summary>
        /// The <see cref="AVPacket.ReadFrame(AVFormatContext)"/> reads the frame.
        /// </summary>
        [Fact]
        public void ReadFrame_ReadsFrame()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.ReadFrame(It.IsAny<AVFormatContext>(), It.IsAny<AVPacket>()))
                .Returns(0)
                .Verifiable();

            var ffmpegClient = ffmpegMock.Object;

            using (var packet = new AVPacket(ffmpegClient))
            {
                Assert.True(packet.ReadFrame(new AVFormatContext()));
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVPacket.ReadFrame(AVFormatContext)"/> returns false.
        /// </summary>
        [Fact]
        public void ReadFrame_AVERROR_EOF_ReturnsFalse()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.ReadFrame(It.IsAny<AVFormatContext>(), It.IsAny<AVPacket>()))
                .Returns(NativeFFmpeg.AVERROR_EOF)
                .Verifiable();

            var ffmpegClient = ffmpegMock.Object;

            using (var packet = new AVPacket(ffmpegClient))
            {
                Assert.False(packet.ReadFrame(new AVFormatContext()));
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVPacket.ReadFrame(AVFormatContext)"/> throws.
        /// </summary>
        [Fact]
        public void ReadFrame_AVError_Throws()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.ReadFrame(It.IsAny<AVFormatContext>(), It.IsAny<AVPacket>()))
                .Returns(-100)
                .Verifiable();
            ffmpegMock
                .Setup(c => c.ThrowOnAVError(-100, false))
                .Verifiable();

            var ffmpegClient = ffmpegMock.Object;

            using (var packet = new AVPacket(ffmpegClient))
            {
                Assert.False(packet.ReadFrame(new AVFormatContext()));
            }

            ffmpegMock.Verify();
        }
    }
}
