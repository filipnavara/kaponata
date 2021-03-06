// <copyright file="AVFormatContextHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to an <see cref="NativeAVFormatContext"/>.
    /// </summary>
    public class AVFormatContextHandle : SafeHandle
    {
        private readonly FFmpegClient ffmpeg;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContextHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// The ffmpeg client.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        public unsafe AVFormatContextHandle(FFmpegClient ffmpeg, NativeAVFormatContext* context)
        : base((IntPtr)context, true)
        {
            this.ffmpeg = ffmpeg;
        }

        /// <inheritdoc/>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override unsafe bool ReleaseHandle()
        {
            this.ffmpeg.FreeAVFormatContext(this.handle);
            return true;
        }
    }
}
