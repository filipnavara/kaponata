// <copyright file="ScrCpyOptions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Class containting the scrcpy server options.
    /// </summary>
    /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/server/src/main/java/com/genymobile/scrcpy/Server.java"/>
    /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/src/server.c#L267"/>
    /// for default options <seealso href="https://github.com/Genymobile/scrcpy/blob/c5c5fc18aec537a5c4134a5bcd45e9f8bbadb66d/app/src/scrcpy.h"/> and <see href="https://github.com/Genymobile/scrcpy/blob/c5c5fc18aec537a5c4134a5bcd45e9f8bbadb66d/app/meson.build"/>
    /// 
    public partial class ScrCpyOptions
    {
        /// <summary>
        /// Gets or sets the version of scrcpy.
        /// </summary>
        public Version Version { get; set; } = new Version(1, 17);

        /// <summary>
        /// Gets or sets the requested loglevel.
        /// </summary>
        public ScrCpyLogLevel LogLevel { get; set; } = ScrCpyLogLevel.INFO;

        /// <summary>
        /// gets or sets the max video size for both dimensions, in pixels. This should be a multiple of 8.
        ///  0: unlimited.
        /// </summary>
        public int MaxSize { get; set; } = 0;

        /// <summary>
        /// Gets or sets the bit rate. default is 8Mbps.
        /// </summary>
        public int BitRate { get; set; } = 8000000;

        /// <summary>
        /// Gets or sets the maximum frames per second.
        /// </summary>
        public int MaxFps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the locked video orientation.
        /// -1: unlocked; natural device orientation is 0 and each increment adds 90 degrees counterclockwise.
        /// </summary>
        public int LockedVideoOrientation { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether the tunnel forward option should be enabled.
        /// </summary>
        public bool TunnelForward { get; set; } = false;

        /// <summary>
        /// Gets or sets the crop area. Rect format width:height:x:y or - for no crop.
        /// </summary>
        public string Rectangle { get; set; } = "-";

        /// <summary>
        /// gets or sets a value indicating whether frame media should be enabled. (packet boundaries + timestamp).
        /// </summary>
        public bool FrameMeta { get; set; } = true;

        /// <summary>
        /// gets or sets a value indicating whether control should be enabled.
        /// </summary>
        public bool Control { get; set; } = false;

        /// <summary>
        /// Gets or sets the display id.
        /// </summary>
        public int DisplayId { get; set; } = 0;

        /// <summary>
        /// gets or sets a value indicating whether touches should be displayed.
        /// </summary>
        public bool ShowTouches { get; set; } = false;

        /// <summary>
        /// gets or sets a value indicating whether the stay awake option should be enabled.
        /// </summary>
        public bool StayAwake { get; set; } = false;

        /// <summary>
        /// Gets or sets the codec options.
        /// </summary>
        public string CodecOptions { get; set; } = "-";

        /// <summary>
        /// Gets or sets the encorder name.
        /// </summary>
        public string EncoderName { get; set; } = "-";
    }
}
