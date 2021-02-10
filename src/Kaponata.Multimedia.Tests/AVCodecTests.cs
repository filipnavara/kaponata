// <copyright file="AVCodecTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using AVCodecID = FFmpeg.AutoGen.AVCodecID;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;

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
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVCodec.AVCodec(AVCodecHandle)"/> initializes the  instance.
        /// </summary>s
        [Fact]
        public void Constructor_InitializesInstance()
        {
            NativeAVCodec nativeCodec = new NativeAVCodec
            {
            };

            var handle = new AVCodecHandle(&nativeCodec);

            var codec = new AVCodec(handle);

            Assert.Equal((int)&nativeCodec, (int)codec.NativeObject);
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

                var handle = new AVCodecHandle(&nativeCodec);
                var codec = new AVCodec(handle);

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

                var handle = new AVCodecHandle(&nativeCodec);
                var codec = new AVCodec(handle);

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

                var handle = new AVCodecHandle(&nativeCodec);
                var codec = new AVCodec(handle);

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

                var handle = new AVCodecHandle(&nativeCodec);
                var codec = new AVCodec(handle);

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

            var handle = new AVCodecHandle(&nativeCodec);
            var codec = new AVCodec(handle);

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

            var handle = new AVCodecHandle(&nativeCodec);
            var codec = new AVCodec(handle);

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

                var handle = new AVCodecHandle(&nativeCodec);
                var codec = new AVCodec(handle);

                Assert.Equal("test (test)", codec.ToString());
            }
        }

        /// <summary>
        /// Tests fetching a codec and accessing its properties.
        /// </summary>
        [Fact]
        public void GetCodecPropertiesTest()
        {
            var ffmpeg = new FFMpegClient();

            using (var codec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264))
            {
                Assert.Equal("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", codec.LongName);
                Assert.Equal("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (h264)", codec.ToString());
                Assert.Equal("h264", codec.Name);
                Assert.Equal(AVCodecCapabilities.DR1 | AVCodecCapabilities.Delay | AVCodecCapabilities.Threads | AVCodecCapabilities.SliceThreads, codec.Capabilities);
                Assert.True(codec.IsDecoder);
                Assert.False(codec.IsEncoder);
            }
        }

        /// <summary>
        /// Makes sure the required H.264 codecs are available.
        /// </summary>
        [Fact]
        public void HasRequiredCodecsTest()
        {
            var ffmpeg = new FFMpegClient();

            var codecs = ffmpeg.GetAvailableCodecs();
            var h264Decoders = codecs.Where(c => c.Id == AVCodecID.AV_CODEC_ID_H264 && c.IsDecoder).ToArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Assert.Collection(
                    h264Decoders,
                    d => Assert.Equal("h264", d.Name));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Assert.Collection(
                    h264Decoders,
                    d => Assert.Equal("h264", d.Name),
                    d => Assert.Equal("h264_v4l2m2m", d.Name));
            }
            else
            {
                Assert.Collection(
                    h264Decoders,
                    d => Assert.Equal("h264", d.Name),
                    d => Assert.Equal("h264_qsv", d.Name),
                    d => Assert.Equal("h264_cuvid", d.Name));
            }
        }
    }
}
