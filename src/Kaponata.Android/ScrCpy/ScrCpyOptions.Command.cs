// <copyright file="ScrCpyOptions.Command.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Linq;

namespace Kaponata.Android.ScrCpy
{
    /// <summary>
    /// Contains the command method for the <see cref="ScrCpyOptions"/>.
    /// </summary>
    public partial class ScrCpyOptions
    {
        /// <summary>
        /// Gets the default scrcpy server options.
        /// </summary>
        public static readonly ScrCpyOptions DefaultOptions = new ScrCpyOptions();

        /// <summary>
        /// Creates the scrcpy server command based on the values of this instance.
        /// </summary>
        /// <param name="path">
        /// The path of the scrcpy server.
        /// </param>
        /// <returns>
        /// The command used to run the scrcpy server.
        /// </returns>
        public string GetCommand(string path)
        {
            var arguments = new object[]
            {
                this.Version,
                this.LogLevel,
                this.MaxSize,
                this.BitRate,
                this.MaxFps,
                this.LockedVideoOrientation,
                this.TunnelForward,
                this.Rectangle,
                this.FrameMeta,
                this.Control,
                this.DisplayId,
                this.ShowTouches,
                this.StayAwake,
                this.CodecOptions,
                this.EncoderName,
            };

            var argumentsString = string.Join(" ", arguments.Select(a => a.ToString()));

            return $"CLASSPATH={path} app_process / com.genymobile.scrcpy.Server {argumentsString}";
        }
    }
}
