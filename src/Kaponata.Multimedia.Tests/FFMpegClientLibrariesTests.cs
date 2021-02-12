// <copyright file="FFMpegClientLibrariesTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using System;
using System.IO;
using Xunit;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the libraries methods of the <see cref="FFMpegClient"/> class.
    /// </summary>
    public class FFMpegClientLibrariesTests
    {
        /// <summary>
        /// The <see cref="FFMpegClient.GetNativePath(string, bool)"/> returns the native library path.
        /// </summary>
        /// <param name="name">
        /// The name of the ffmpeg library.
        /// </param>
        /// <param name="expectedFileName">
        /// The expected file name.
        /// </param>
        [Theory]
        [InlineData("avcodec", "libavcodec.so.58")]
        [InlineData("avdevice", "libavdevice.so.58")]
        [InlineData("avfilter", "libavfilter.so.7")]
        [InlineData("avformat", "libavformat.so.58")]
        [InlineData("avutil", "libavutil.so.56")]
        [InlineData("swresample", "libswresample.so.3")]
        [InlineData("swscale", "libswscale.so.5")]
        public void GetLibraryPath_ReturnsPath(string name, string expectedFileName)
        {
            Assert.Equal(expectedFileName, FFMpegClient.GetNativePath(name, false));
        }

        /// <summary>
        /// The <see cref="FFMpegClient.GetNativeVersion(string)"/> throws an exception when the package is unknown.
        /// </summary>
        [Fact]
        public void GetNativeVersion_ThrowsOnUnkownLibrary()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>("name", () => FFMpegClient.GetNativeVersion("test123"));
            Assert.Contains("test123", exception.Message);
        }

        /// <summary>
        /// The <see cref="FFMpegClient.GetOrLoadLibrary(string, Func{string, string}, Func{string, IntPtr})"/> loads the library.
        /// </summary>
        [Fact]
        public void GetOrLoadLibrary_LoadsLibrary()
        {
            Func<string, IntPtr> load = (name) => new IntPtr(12);
            Func<string, string> getNativePath = (name) => name;
            Assert.Equal(12, FFMpegClient.GetOrLoadLibrary("test", getNativePath, load).ToInt32());
            var entry = Assert.Single(FFMpegClient.LibraryHandles, (e) => e.Key == "test");
            Assert.Equal(12, entry.Value.ToInt32());

            Assert.Equal(12, FFMpegClient.GetOrLoadLibrary("test", getNativePath, load).ToInt32());
            entry = Assert.Single(FFMpegClient.LibraryHandles, (e) => e.Key == "test");
            Assert.Equal(12, entry.Value.ToInt32());
        }

        /// <summary>
        /// The <see cref="FFMpegClient.GetOrLoadLibrary(string)"/> loads the ffmpeg library.
        /// </summary>
        /// <param name="libraryName">
        /// The library name.
        /// </param>
        [Theory]
        [InlineData("avutil")]
        [InlineData("avcodec")]
        [InlineData("avformat")]
        [InlineData("swscale")]
        [InlineData("swresample")]
        public void GetOrLoadLibrary_LoadsFFMpegLibrary(string libraryName)
        {
            Assert.NotEqual(0, FFMpegClient.GetOrLoadLibrary(libraryName).ToInt64());
        }

        /// <summary>
        /// The <see cref="FFMpegClient.Initialize"/> method loads the ffmpeg libraries.
        /// </summary>
        [Fact]
        public void Initialize_LoadsLibaries()
        {
            FFMpegClient.Initialize();
            FFMpegClient.Initialize();

            FFmpeg.AutoGen.ffmpeg.av_version_info();
            Assert.True(FFMpegClient.LibraryHandles.Count > 0);
        }
    }
}
