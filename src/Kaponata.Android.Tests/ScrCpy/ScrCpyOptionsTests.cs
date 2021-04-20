// <copyright file="ScrCpyOptionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.ScrCpy;
using System;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="ScrCpyOptions"/> class.
    /// </summary>
    public class ScrCpyOptionsTests
    {
        /// <summary>
        /// The <see cref="ScrCpyOptions.DefaultOptions"/> returns the default options.
        /// </summary>
        [Fact]
        public void DefaultOptions()
        {
            var options = ScrCpyOptions.DefaultOptions;

            Assert.Equal(8000000, options.BitRate);
            Assert.Equal("-", options.CodecOptions);
            Assert.False(options.Control);
            Assert.Equal(0, options.DisplayId);
            Assert.Equal("-", options.EncoderName);
            Assert.True(options.FrameMeta);
            Assert.Equal(-1, options.LockedVideoOrientation);
            Assert.Equal(ScrCpyLogLevel.INFO, options.LogLevel);
            Assert.Equal(0, options.MaxFps);
            Assert.Equal(0, options.MaxSize);
            Assert.Equal("-", options.Rectangle);
            Assert.False(options.ShowTouches);
            Assert.False(options.StayAwake);
            Assert.False(options.TunnelForward);
            Assert.Equal(new Version(1, 17), options.Version);
        }

        /// <summary>
        /// The <see cref="ScrCpyOptions.GetCommand(string)"/> returns the scrcpy command.
        /// </summary>
        [Fact]
        public void GetCommand()
        {
            var options = ScrCpyOptions.DefaultOptions;
            Assert.Equal("CLASSPATH=test app_process / com.genymobile.scrcpy.Server 1.17 INFO 0 8000000 0 -1 False - True False 0 False False - -", options.GetCommand("test"));
        }
    }
}
