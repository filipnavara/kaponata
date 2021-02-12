// <copyright file="AVFormatContextTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;
using NativeAVStream = FFmpeg.AutoGen.AVStream;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVFormatContext"/> class.
    /// </summary>
    public unsafe class AVFormatContextTests
    {
        /// <summary>
        /// The <see cref="AVFormatContext.AVFormatContext(FFMpegClient, AVFormatContextHandle)"/> initializes the new instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var codecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
            };

            var nativeStream = new NativeAVStream
            {
                codecpar = &codecParameters,
            };

            var streamPtr = new IntPtr(&nativeStream);

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                duration = 10,
                nb_streams = 1,
                event_flags = (int)AVFormatContextEventFlags.MetadataUpdated,
                ctx_flags = (int)AVFormatContextFlags.NoHeader,
                streams = (NativeAVStream**)streamPtr,
            };

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                Assert.Equal(handle, formatContext.Handle);
                Assert.Equal<uint>(1, formatContext.StreamCount);
                Assert.False(formatContext.IsClosed);
                Assert.False(handle.IsClosed);
                Assert.Equal((int)AVFormatContextEventFlags.MetadataUpdated, (int)formatContext.EventFlags);
                Assert.Equal((int)AVFormatContextFlags.NoHeader, (int)formatContext.Flags);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.NativeObject"/> sets the new handle.
        /// </summary>
        [Fact]
        public void NativeObject_SetsNewHandle()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var nativeAVFormatContext1 = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            var nativeAVFormatContext2 = new NativeAVFormatContext
            {
                nb_streams = 2,
            };

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            using (var handle1 = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext1))
            using (var formatContext = new AVFormatContext(ffmpeg, handle1))
            {
                Assert.Equal(handle1, formatContext.Handle);
                Assert.Equal<uint>(1, formatContext.StreamCount);
                Assert.Equal((int)&nativeAVFormatContext1, (int)formatContext.NativeObject);
                formatContext.NativeObject = &nativeAVFormatContext2;
                Assert.Equal<uint>(2, formatContext.StreamCount);
                Assert.Equal((int)&nativeAVFormatContext2, (int)formatContext.NativeObject);

                Assert.True(handle1.IsClosed);
                Assert.False(formatContext.Handle.IsClosed);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.IOContext"/> returns a <see cref="AVIOContext"/>.
        /// </summary>
        [Fact]
        public void IOContext_Get()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var bytes = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };

            var bytesHandle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, bytesHandle, bytes.Length);

            var nativeIOContext = new NativeAVIOContext
            {
                buffer = (byte*)bytesHandle,
                pos = 5,
            };

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                pb = &nativeIOContext,
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                var ioContext = formatContext.IOContext;
                var result = new byte[4];
                Marshal.Copy((IntPtr)ioContext.Buffer.NativeObject, result, 0, bytes.Length);
                Assert.Equal(bytes, result);
            }

            ffmpegMock.Verify();

            Marshal.FreeHGlobal(bytesHandle);
        }

        /// <summary>
        /// The <see cref="AVFormatContext.IOContext"/> sets the <see cref="AVIOContext"/>.
        /// </summary>
        [Fact]
        public void IOContext_Set()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var bytes = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };

            var bytesHandle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, bytesHandle, bytes.Length);

            var nativeIOContext = new NativeAVIOContext
            {
                buffer = (byte*)bytesHandle,
                pos = 5,
            };

            var nativeAVFormatContext = new NativeAVFormatContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            using (var avioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            {
                formatContext.IOContext = avioContext;

                var ioContext = formatContext.IOContext;
                var result = new byte[4];
                Marshal.Copy((IntPtr)ioContext.Buffer.NativeObject, result, 0, bytes.Length);
                Assert.Equal(bytes, result);
            }

            ffmpegMock.Verify();

            Marshal.FreeHGlobal(bytesHandle);
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetStreamCodecType(int)"/> returns the codec type for the stream on given index.
        /// </summary>
        [Fact]
        public void GetStreamCodecType_ReturnsStream()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var codecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
            };

            var nativeStream = new NativeAVStream
            {
                codecpar = &codecParameters,
            };
            var streamPtr = new IntPtr(&nativeStream);

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
                streams = (NativeAVStream**)&streamPtr,
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                var type = formatContext.GetStreamCodecType(0);
                Assert.Equal(NativeAVMediaType.AVMEDIA_TYPE_VIDEO, type);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetStreamCodecType(int)"/> throws when an illegal index is provided.
        /// </summary>
        [Fact]
        public void GetStreamCodecType_ThrowsOnIllegalIndex()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStreamCodecType(20));
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStreamCodecType(-1));
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetStream(int)"/> returns the stream on the given index.
        /// </summary>
        [Fact]
        public void GetStream_ReturnsStream()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var codecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
            };

            var nativeStream = new NativeAVStream
            {
                codecpar = &codecParameters,
            };
            var streamPtr = new IntPtr(&nativeStream);

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
                streams = (NativeAVStream**)&streamPtr,
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                var stream = formatContext.GetStream(0);
                Assert.Equal(NativeAVMediaType.AVMEDIA_TYPE_VIDEO, stream.CodecParameters.MediaType);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetStream(int)"/> throws when an illegal index is provided.
        /// </summary>
        [Fact]
        public void GetStream_ThrowsOnIllegalIndex()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var formatContext = new AVFormatContext(ffmpeg, handle))
            {
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStream(20));
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStream(-1));
            }

            ffmpegMock.Verify();
        }
    }
}
