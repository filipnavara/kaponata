// <copyright file="AVCodecParametersTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Xunit;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVCodecParameters"/> class.
    /// </summary>
    public unsafe class AVCodecParametersTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodecParametersTests"/> class.
        /// </summary>
        public AVCodecParametersTests()
        {
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// THe <see cref="AVCodecParameters.MediaType"/> returns the native MediaType.
        /// </summary>
        [Fact]
        public void MediaType_ReturnsNativeMediaType()
        {
            var nativeCodecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
            };

            var codecParameters = new AVCodecParameters(&nativeCodecParameters);

            Assert.Equal(NativeAVMediaType.AVMEDIA_TYPE_VIDEO, codecParameters.MediaType);
        }
    }
}
