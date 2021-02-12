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
        /// <param name="isWindows">
        /// A value indicating whether the os platform is windows.
        /// </param>
        /// <param name="expectedFileName">
        /// The expected file name.
        /// </param>
        [Theory]
        [InlineData("libwinpthread", true, "libwinpthread-1.dll")]
        [InlineData("avcodec", true, "avcodec-58.dll")]
        [InlineData("avcodec", false, "libavcodec.so")]
        public void GetLibraryPath_ReturnsPath(string name, bool isWindows, string expectedFileName)
        {
            var expectedPath = expectedFileName;
            if (isWindows)
            {
                expectedPath = Path.Combine(Path.GetFullPath("."), "runtimes", "win7-x64", "native", expectedPath);
            }

            Assert.Equal(expectedPath, FFMpegClient.GetNativePath(name, isWindows));
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

            Assert.Equal("4.3.1", FFmpeg.AutoGen.ffmpeg.av_version_info());
            Assert.True(FFMpegClient.LibraryHandles.Count > 0);
        }
    }
}
