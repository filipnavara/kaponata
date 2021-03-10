// <copyright file="AVCodecTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Moq;
using System;
using Xunit;
using AVCodecID = FFmpeg.AutoGen.AVCodecID;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;
using NativeAVCodecContext = FFmpeg.AutoGen.AVCodecContext;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;
using NativeAVStream = FFmpeg.AutoGen.AVStream;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVCodec"/> class.
    /// </summary>
    public unsafe class AVCodecTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodecTests"/> class.
        /// </summary>
        public AVCodecTests()
        {
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVCodec.AVCodec(FFmpegClient, AVStream)"/> initializes the new instance.
        /// </summary>
        [Fact]
        public void AVCodec_InitializesInstance()
        {
            var nativeCodec = new NativeAVCodec()
            {
                capabilities = (int)AVCodecCapabilities.Truncated,
            };

            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.FindDecoder(AVCodecID.AV_CODEC_ID_H264))
                .Returns((IntPtr)(&nativeCodec))
                .Verifiable();

            var codecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
                codec_id = AVCodecID.AV_CODEC_ID_H264,
            };

            var nativeCodecContext = new NativeAVCodecContext()
            {
                codec_id = AVCodecID.AV_CODEC_ID_H264,
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var nativeStream = new NativeAVStream
            {
                codecpar = &codecParameters,
                codec = &nativeCodecContext,
            };

            var stream = new AVStream(&nativeStream);

            var ffmpeg = ffmpegMock.Object;
            var codec = new AVCodec(ffmpeg, stream);

            Assert.Equal((int)AVCodecCapabilities.Truncated, stream.CodecContext->flags);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// The <see cref="AVCodec.Name"/> property returns the name of the native avcodec.
        /// </summary>
        [Fact]
        public void Name_ReturnsNativeName()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodec = new NativeAVCodec
                {
                    name = p,
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                var ffmpegClient = ffmpegMock.Object;

                var codecContext = new NativeAVCodecContext
                { };

                var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

                Assert.Equal("test", codec.Name);
            }
        }

        /// <summary>
        /// The <see cref="AVCodec.LongName"/> property returns the long name of the native avcodec.
        /// </summary>
        [Fact]
        public void LongName_ReturnsNativeLongName()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodec = new NativeAVCodec
                {
                    long_name = p,
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                var ffmpegClient = ffmpegMock.Object;

                var codecContext = new NativeAVCodecContext
                { };

                var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

                Assert.Equal("test", codec.LongName);
            }
        }

        /// <summary>
        /// The <see cref="AVCodec.IsEncoder"/> property returns whether the native avcodec is an encoder.
        /// </summary>
        [Fact]
        public void IsEncoder_ReturnsNativeIsEncoder()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodec = new NativeAVCodec
                {
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                var ffmpegClient = ffmpegMock.Object;

                var codecContext = new NativeAVCodecContext
                { };

                var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

                Assert.False(codec.IsEncoder);
            }
        }

        /// <summary>
        /// The <see cref="AVCodec.IsDecoder"/> property returns whether the native avcodec is an decoder.
        /// </summary>
        [Fact]
        public void IsEncoder_ReturnsNativeIsDecoder()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodec = new NativeAVCodec
                {
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                var ffmpegClient = ffmpegMock.Object;

                var codecContext = new NativeAVCodecContext
                { };

                var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

                Assert.False(codec.IsDecoder);
            }
        }

        /// <summary>
        /// The <see cref="AVCodec.Capabilities"/> property returns a list of codec capabilities.
        /// </summary>
        [Fact]
        public void Capabilities_ReturnsNativeCapabilities()
        {
            var capabilities = AVCodecCapabilities.DR1 | AVCodecCapabilities.Delay | AVCodecCapabilities.Threads | AVCodecCapabilities.SliceThreads;

            var nativeCodec = new NativeAVCodec
            {
                capabilities = (int)capabilities,
            };

            var ffmpegMock = new Mock<FFmpegClient>();
            var ffmpegClient = ffmpegMock.Object;

            var codecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

            Assert.Equal(capabilities, codec.Capabilities);
        }

        /// <summary>
        /// The <see cref="AVCodec.Id"/> property returns the native id.
        /// </summary>
        [Fact]
        public void Id_ReturnsNativeId()
        {
            var nativeCodec = new NativeAVCodec
            {
                id = AVCodecID.AV_CODEC_ID_4XM,
            };

            var ffmpegMock = new Mock<FFmpegClient>();
            var ffmpegClient = ffmpegMock.Object;

            var codecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

            Assert.Equal(AVCodecID.AV_CODEC_ID_4XM, codec.Id);
        }

        /// <summary>
        /// The <see cref="AVCodec.ToString"/> method returns the string representation of the native descriptor.
        /// </summary>
        [Fact]
        public void ToString_ReturnsString()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodec = new NativeAVCodec
                {
                    name = p,
                    long_name = p,
                };

                var ffmpegMock = new Mock<FFmpegClient>();
                var ffmpegClient = ffmpegMock.Object;

                var codecContext = new NativeAVCodecContext
                { };

                var codec = new AVCodec(ffmpegClient, &codecContext, &nativeCodec);

                Assert.Equal("test (test)", codec.ToString());
            }
        }

        /// <summary>
        /// The <see cref="AVCodec"/> properties return the native values.
        /// </summary>
        [Fact]
        public void Properties_ReturnNativeValues()
        {
            var ffmpeg = new FFmpegClient();

            var nativeCodec = ffmpeg.FindDecoder(AVCodecID.AV_CODEC_ID_H264);
            var nativeCodecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpeg, &nativeCodecContext, (NativeAVCodec*)nativeCodec);

            Assert.Equal("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", codec.LongName);
            Assert.Equal("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (h264)", codec.ToString());
            Assert.Equal("h264", codec.Name);
            Assert.Equal(AVCodecCapabilities.DR1 | AVCodecCapabilities.Delay | AVCodecCapabilities.Threads | AVCodecCapabilities.SliceThreads, codec.Capabilities);
            Assert.True(codec.IsDecoder);
            Assert.False(codec.IsEncoder);
        }

        /// <summary>
        /// The <see cref="AVCodec.SendPacket(AVPacket)"/> sends the packet.
        /// </summary>
        [Fact]
        public void SendPacket_SendsPacket()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.IsCodecOpen(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.IsDecoder(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.SendPacket(It.IsAny<IntPtr>(), It.IsAny<IntPtr>()))
                .Returns(0)
                .Verifiable();

            var ffmpeg = ffmpegMock.Object;

            var nativeCodec = ffmpeg.FindDecoder(AVCodecID.AV_CODEC_ID_H264);
            var nativeCodecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpeg, &nativeCodecContext, (NativeAVCodec*)nativeCodec);
            codec.SendPacket(new AVPacket(ffmpeg));

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVCodec.SendPacket(AVPacket)"/> throws an exception when the codec is not open.
        /// </summary>
        [Fact]
        public void SendPacket_ThrowsOnCodecClosed()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.IsCodecOpen(It.IsAny<IntPtr>()))
                .Returns(false)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.IsDecoder(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.SendPacket(It.IsAny<IntPtr>(), It.IsAny<IntPtr>()))
                .Returns(0)
                .Verifiable();

            var ffmpeg = ffmpegMock.Object;

            var nativeCodec = ffmpeg.FindDecoder(AVCodecID.AV_CODEC_ID_H264);
            var nativeCodecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpeg, &nativeCodecContext, (NativeAVCodec*)nativeCodec);
            Assert.Throws<InvalidOperationException>(() => codec.SendPacket(new AVPacket(ffmpeg)));
        }

        /// <summary>
        /// The <see cref="AVCodec.SendPacket(AVPacket)"/> throws an exception when the codec is no decoder.
        /// </summary>
        [Fact]
        public void SendPacket_ThrowsOnNoDecoder()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.IsCodecOpen(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.IsDecoder(It.IsAny<IntPtr>()))
                .Returns(false)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.SendPacket(It.IsAny<IntPtr>(), It.IsAny<IntPtr>()))
                .Returns(0)
                .Verifiable();

            var ffmpeg = ffmpegMock.Object;

            var nativeCodec = ffmpeg.FindDecoder(AVCodecID.AV_CODEC_ID_H264);
            var nativeCodecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpeg, &nativeCodecContext, (NativeAVCodec*)nativeCodec);
            Assert.Throws<InvalidOperationException>(() => codec.SendPacket(new AVPacket(ffmpeg)));
        }

        /// <summary>
        /// The <see cref="AVCodec.SendPacket(AVPacket)"/> throws an exception when sending the packet fails.
        /// </summary>
        [Fact]
        public void SendPacket_ThrowsOnSendPacketFail()
        {
            var ffmpegMock = new Mock<FFmpegClient>();
            ffmpegMock
                .Setup(c => c.IsCodecOpen(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.IsDecoder(It.IsAny<IntPtr>()))
                .Returns(true)
                .Verifiable();

            ffmpegMock
                .Setup(c => c.SendPacket(It.IsAny<IntPtr>(), It.IsAny<IntPtr>()))
                .Returns(-100)
                .Verifiable();
            ffmpegMock
                .Setup(c => c.ThrowOnAVError(-100, true))
                .Verifiable();

            var ffmpeg = ffmpegMock.Object;

            var nativeCodec = ffmpeg.FindDecoder(AVCodecID.AV_CODEC_ID_H264);
            var nativeCodecContext = new NativeAVCodecContext
            { };

            var codec = new AVCodec(ffmpeg, &nativeCodecContext, (NativeAVCodec*)nativeCodec);
            codec.SendPacket(new AVPacket(ffmpeg));
        }
    }
}
