// <copyright file="AVStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Xunit;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVDictionary = FFmpeg.AutoGen.AVDictionary;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;
using NativeAVRational = FFmpeg.AutoGen.AVRational;
using NativeAVStream = FFmpeg.AutoGen.AVStream;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVStream"/> class.
    /// </summary>
    public unsafe class AVStreamTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVStreamTests"/> class.
        /// </summary>
        public AVStreamTests()
        {
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVStream.CodecParameters"/> property returns the native codex context.
        /// </summary>
        [Fact]
        public void CodecContext_ReturnsNativeCodecContext()
        {
            var codecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
            };

            var nativeStream = new NativeAVStream
            {
                codecpar = &codecParameters,
            };

            var stream = new AVStream(&nativeStream);

            Assert.Equal(NativeAVMediaType.AVMEDIA_TYPE_VIDEO, stream.CodecParameters.Type);
        }

        /// <summary>
        /// The <see cref="AVStream.Index"/> property returns the native index.
        /// </summary>
        [Fact]
        public void Index_ReturnsNativeIndex()
        {
            var nativeStream = new NativeAVStream
            {
                index = 6,
            };

            var stream = new AVStream(&nativeStream);

            Assert.Equal(6, stream.Index);
        }

        /// <summary>
        /// The <see cref="AVStream.TimeBase"/> property returns the native time base.
        /// </summary>
        [Fact]
        public void TimeBase_ReturnsNativeTimeBase()
        {
            var nativeTimeBase = new NativeAVRational
            {
                den = 100,
                num = 4,
            };

            var nativeStream = new NativeAVStream
            {
                time_base = nativeTimeBase,
            };

            var stream = new AVStream(&nativeStream);

            Assert.Equal(4, stream.TimeBase.num);
            Assert.Equal(100, stream.TimeBase.den);
        }

        /// <summary>
        /// The <see cref="AVStream.Metadata"/> property returns the native meta data.
        /// </summary>
        [Fact]
        public void Metadata_ReturnsNativeMetadata()
        {
            var nativeDictionary = new NativeAVDictionary
            {
            };

            var nativeStream = new NativeAVStream
            {
                metadata = &nativeDictionary,
            };

            var stream = new AVStream(&nativeStream);

            Assert.Empty(stream.Metadata);
        }

        /// <summary>
        /// The <see cref="AVStream.Metadata"/> property returns null in case a null dictionary is given.
        /// </summary>
        [Fact]
        public void Metadata_ReturnsNull()
        {
            var nativeStream = new NativeAVStream
            {
                metadata = null,
            };

            var stream = new AVStream(&nativeStream);

            Assert.Null(stream.Metadata);
        }
    }
}
