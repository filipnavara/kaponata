// <copyright file="FFmpegClientLibrariesTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using System;
using Xunit;
using NativeFFmpeg = FFmpeg.AutoGen.ffmpeg;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the libraries methods of the <see cref="FFmpegClient"/> class.
    /// </summary>
    public class FFmpegClientLibrariesTests
    {
        /// <summary>
        /// The <see cref="FFmpegClient.GetNativePath(string, bool)"/> returns the native library path.
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
            Assert.Equal(expectedFileName, FFmpegClient.GetNativePath(name, false));
        }

        /// <summary>
        /// The <see cref="FFmpegClient.GetNativeVersion(string)"/> returns the version number.
        /// </summary>
        /// <param name="name">
        /// The name of the ffmpeg library.
        /// </param>
        /// <param name="version">
        /// The expected version number.
        /// </param>
        [Theory]
        [InlineData("avcodec", 58)]
        [InlineData("avdevice", 58)]
        [InlineData("avfilter", 7)]
        [InlineData("avformat", 58)]
        [InlineData("avutil", 56)]
        [InlineData("swresample", 3)]
        [InlineData("swscale", 5)]
        [InlineData("libwinpthread", 1)]
        public void GetNativeVersion_ReturnsVersion(string name, int version)
        {
            Assert.Equal(version, FFmpegClient.GetNativeVersion(name));
        }

        /// <summary>
        /// The <see cref="FFmpegClient.ThrowOnAVError(int, bool)"/> retruns on success.
        /// </summary>
        /// <param name="ret">
        /// The return value based on which to determine whether a <see cref="InvalidOperationException"/> should be thrown.
        /// </param>
        /// <param name="postiveIndicatesSuccess">
        /// <see langword="true"/> if positive numbers indicate success; <see langword="false"/> if only strictly zero values
        /// indicate success. Negative values always indicate a failure.
        /// </param>
        [Theory]
        [InlineData(0, true)]
        [InlineData(0, false)]
        [InlineData(1000, true)]
        public void ThrowOnAVError_ReturnsOnSuccess(int ret, bool postiveIndicatesSuccess)
        {
            var client = new FFmpegClient();
            client.ThrowOnAVError(ret, postiveIndicatesSuccess);
        }

        /// <summary>
        /// The <see cref="FFmpegClient.ThrowOnAVError(int, bool)"/> throws on error.
        /// </summary>
        [Fact]
        public void ThrowOnAVError_ThrowsOnError()
        {
            var client = new FFmpegClient();
            Assert.Throws<Exception>(() => client.ThrowOnAVError(-100, true));
            Assert.Throws<InvalidOperationException>(() => client.ThrowOnAVError(100, false));
            Assert.Throws<Exception>(() => client.ThrowOnAVError(-100, false));
        }

        /// <summary>
        /// The <see cref="FFmpegClient.GetNativeVersion(string)"/> throws an exception when the package is unknown.
        /// </summary>
        [Fact]
        public void GetNativeVersion_ThrowsOnUnkownLibrary()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>("name", () => FFmpegClient.GetNativeVersion("test123"));
            Assert.Contains("test123", exception.Message);
        }

        /// <summary>
        /// The <see cref="FFmpegClient.GetOrLoadLibrary(string, Func{string, string}, Func{string, IntPtr})"/> loads the library.
        /// </summary>
        [Fact]
        public void GetOrLoadLibrary_LoadsLibrary()
        {
            Func<string, IntPtr> load = (name) => new IntPtr(12);
            Func<string, string> getNativePath = (name) => name;
            Assert.Equal(12, FFmpegClient.GetOrLoadLibrary("test", getNativePath, load).ToInt32());
            var entry = Assert.Single(FFmpegClient.LibraryHandles, (e) => e.Key == "test");
            Assert.Equal(12, entry.Value.ToInt32());

            Assert.Equal(12, FFmpegClient.GetOrLoadLibrary("test", getNativePath, load).ToInt32());
            entry = Assert.Single(FFmpegClient.LibraryHandles, (e) => e.Key == "test");
            Assert.Equal(12, entry.Value.ToInt32());
        }

        /// <summary>
        /// The <see cref="FFmpegClient.GetOrLoadLibrary(string)"/> loads the ffmpeg library.
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
        public void GetOrLoadLibrary_LoadsFFmpegLibrary(string libraryName)
        {
            Assert.NotEqual(0, FFmpegClient.GetOrLoadLibrary(libraryName).ToInt64());
        }

        /// <summary>
        /// The <see cref="FFmpegClient.Initialize"/> method loads the ffmpeg libraries.
        /// </summary>
        [Fact]
        public void Initialize_LoadsLibaries()
        {
            FFmpegClient.Initialize();
            FFmpegClient.Initialize();

            NativeFFmpeg.av_version_info();
            Assert.True(FFmpegClient.LibraryHandles.Count > 0);
        }
    }
}
