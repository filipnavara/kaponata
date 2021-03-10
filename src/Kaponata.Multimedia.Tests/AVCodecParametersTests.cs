// <copyright file="AVCodecParametersTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using Xunit;
using NativeAVCodecID = FFmpeg.AutoGen.AVCodecID;
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
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVCodecParameters"/> properties return the native values.
        /// </summary>
        [Fact]
        public void Properties_ReturnsNativeValues()
        {
            var nativeCodecParameters = new NativeAVCodecParameters
            {
                codec_type = NativeAVMediaType.AVMEDIA_TYPE_VIDEO,
                codec_id = NativeAVCodecID.AV_CODEC_ID_4XM,
                format = 124,
            };

            var codecParameters = new AVCodecParameters(&nativeCodecParameters);

            Assert.Equal(NativeAVMediaType.AVMEDIA_TYPE_VIDEO, codecParameters.Type);
            Assert.Equal(NativeAVCodecID.AV_CODEC_ID_4XM, codecParameters.Id);
            Assert.Equal(124, codecParameters.Format);
        }
    }
}
