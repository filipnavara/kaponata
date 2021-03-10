// <copyright file="AVFormatContextTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
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
        /// The <see cref="AVFormatContext.AVFormatContext(FFmpegClient, AVIOContext, AVFormatContextHandle)"/> initializes the new instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

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

            var nativeIOContext = new NativeAVIOContext
            {
            };

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
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
        /// The <see cref="AVFormatContext.IOContext"/> returns a <see cref="AVIOContext"/>.
        /// </summary>
        [Fact]
        public void IOContext_Get()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

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
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                var result = new byte[4];
                Marshal.Copy((IntPtr)formatContext.IOContext.Buffer.NativeObject, result, 0, bytes.Length);
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
            var ffmpegMock = new Mock<FFmpegClient>();

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

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
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
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
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
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var codecParameters = new NativeAVCodecParameters
            {
                format = 12346,
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

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                var stream = formatContext.GetStream(0);
                Assert.Equal(12346, stream.CodecParameters.Format);
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetStream(int)"/> throws when an illegal index is provided.
        /// </summary>
        [Fact]
        public void GetStream_ThrowsOnIllegalIndex()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStream(20));
                Assert.Throws<ArgumentOutOfRangeException>("index", () => formatContext.GetStream(-1));
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetVideoStream"/> throws when the stream is no video stream.
        /// </summary>
        [Fact]
        public void GetVideoStream_ThrowsOnWrongType()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var codecParameters = new NativeAVCodecParameters
            {
                format = 12346,
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_AUDIO,
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

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                Assert.Throws<InvalidOperationException>(() => formatContext.GetVideoStream());
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.GetVideoStream"/> throws when more than one stream is available.
        /// </summary>
        [Fact]
        public void GetVideoStream_ThrowsOnMultipleStreams()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.FreeAVFormatContext(It.IsAny<IntPtr>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 2,
            };

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                Assert.Throws<InvalidOperationException>(() => formatContext.GetVideoStream());
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.Close"/> method closes the opened input AVFormatContext.
        /// </summary>
        [Fact]
        public void Close_Closes()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.CloseFormatContext(It.IsAny<AVFormatContext>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            using (var ioContext = new AVIOContext(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext)))
            using (var formatContext = new AVFormatContext(ffmpeg, ioContext, handle))
            {
                formatContext.Close();
            }

            ffmpegMock.Verify();
        }

        /// <summary>
        /// The <see cref="AVFormatContext.Dispose"/> does return if already disposed.
        /// </summary>
        [Fact]
        public void Disposes_Twice()
        {
            var ffmpegMock = new Mock<FFmpegClient>();

            ffmpegMock
                .Setup(c => c.CloseFormatContext(It.IsAny<AVFormatContext>()))
                .Verifiable();
            var ffmpeg = ffmpegMock.Object;

            var nativeAVFormatContext = new NativeAVFormatContext
            {
                nb_streams = 1,
            };

            var nativeIOContext = new NativeAVIOContext
            {
            };

            using (var handle = new AVFormatContextHandle(ffmpeg, &nativeAVFormatContext))
            {
                var ioContextMock = new Mock<AVIOContext>(ffmpeg, new AVIOContextHandle(ffmpeg, &nativeIOContext))
                {
                    CallBase = true,
                };

                var formatContext = new AVFormatContext(ffmpeg, ioContextMock.Object, handle);

                formatContext.Dispose();
                formatContext.Dispose();

                ioContextMock.Verify(c => c.Dispose(), Times.Once);
            }
        }
    }
}
