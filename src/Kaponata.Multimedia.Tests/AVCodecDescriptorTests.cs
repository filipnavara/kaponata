// <copyright file="AVCodecDescriptorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using System.Linq;
using Xunit;
using AVCodecID = FFmpeg.AutoGen.AVCodecID;
using AVMediaType = FFmpeg.AutoGen.AVMediaType;
using NativeCodecDescriptor = FFmpeg.AutoGen.AVCodecDescriptor;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVCodecDescriptor"/> class.
    /// </summary>
    public unsafe class AVCodecDescriptorTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodecDescriptorTests"/> class.
        /// </summary>
        public AVCodecDescriptorTests()
        {
            FFmpegClient.Initialize();
        }

        /// <summary>
        /// Tests the <see cref="AVCodecDescriptor"/> properties.
        /// </summary>
        [Fact]
        public void TestCodecProperties()
        {
            var ffmpeg = new FFmpegClient();

            var codecs = ffmpeg.GetAvailableCodecDescriptors();
            var descriptor = codecs.Single(c => c.Id == AVCodecID.AV_CODEC_ID_012V);

            Assert.Equal(AVCodecID.AV_CODEC_ID_012V, descriptor.Id);
            Assert.Equal(AVCodecProps.INTRA_ONLY | AVCodecProps.LOSSLESS, descriptor.Props);
            Assert.Equal("Uncompressed 4:2:2 10-bit", descriptor.LongName);
            Assert.Equal("012v", descriptor.Name);
            Assert.Equal("012v", descriptor.ToString());
            Assert.Equal(AVMediaType.AVMEDIA_TYPE_VIDEO, descriptor.Type);
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.Name"/> property returns the name of the native descriptor.
        /// </summary>
        [Fact]
        public void Name_ReturnsNativeName()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodecDescriptor = new NativeCodecDescriptor
                {
                    name = p,
                };

                var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

                Assert.Equal("test", descriptor.Name);
            }
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.LongName"/> property returns the long name of the native descriptor.
        /// </summary>
        [Fact]
        public void LongName_ReturnsNativeLongName()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodecDescriptor = new NativeCodecDescriptor
                {
                    long_name = p,
                };

                var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

                Assert.Equal("test", descriptor.LongName);
            }
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.Id"/> property returns the id of the native descriptor.
        /// </summary>
        [Fact]
        public void Id_ReturnsNativeId()
        {
            var nativeCodecDescriptor = new NativeCodecDescriptor
            {
                id = AVCodecID.AV_CODEC_ID_4XM,
            };

            var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

            Assert.Equal(AVCodecID.AV_CODEC_ID_4XM, descriptor.Id);
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.Props"/> property returns the properties of the native descriptor.
        /// </summary>
        [Fact]
        public void Props_ReturnsNativeProps()
        {
            var nativeCodecDescriptor = new NativeCodecDescriptor
            {
                props = (int)(AVCodecProps.INTRA_ONLY | AVCodecProps.LOSSLESS),
            };

            var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

            Assert.Equal(AVCodecProps.INTRA_ONLY | AVCodecProps.LOSSLESS, descriptor.Props);
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.Type"/> property returns the type of the native descriptor.
        /// </summary>
        [Fact]
        public void Type_ReturnsNativeType()
        {
            var nativeCodecDescriptor = new NativeCodecDescriptor
            {
                type = AVMediaType.AVMEDIA_TYPE_ATTACHMENT,
            };

            var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

            Assert.Equal(AVMediaType.AVMEDIA_TYPE_ATTACHMENT, descriptor.Type);
        }

        /// <summary>
        /// The <see cref="AVCodecDescriptor.ToString"/> returns the string representation of the native descriptor.
        /// </summary>
        [Fact]
        public void ToString_ReturnsString()
        {
            var name = new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' };
            fixed (byte* p = name)
            {
                var nativeCodecDescriptor = new NativeCodecDescriptor
                {
                    name = p,
                };

                var descriptor = new AVCodecDescriptor(&nativeCodecDescriptor);

                Assert.Equal("test", descriptor.ToString());
            }
        }
    }
}
