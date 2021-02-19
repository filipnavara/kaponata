// <copyright file="AVFrameHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVFrame = FFmpeg.AutoGen.AVFrame;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to an <see cref="NativeAVFrame"/>.
    /// </summary>
    public unsafe class AVFrameHandle : SafeHandle
    {
        private readonly FFmpegClient ffmpeg;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFrameHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFmpegClient"/> interface, which allows releasing
        /// the handle.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        public AVFrameHandle(FFmpegClient ffmpeg, NativeAVFrame* context)
            : base((IntPtr)context, true)
        {
            this.ffmpeg = ffmpeg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFrameHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFmpegClient"/> interface, which allows releasing
        /// the handle.
        /// </param>
        public AVFrameHandle(FFmpegClient ffmpeg)
            : base((IntPtr)ffmpeg.AllocFrame(), true)
        {
            this.ffmpeg = ffmpeg;
        }

        /// <inheritdoc/>
        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        /// <inheritdoc/>
        protected override unsafe bool ReleaseHandle()
        {
            this.ffmpeg.FreeFrame(this.handle);
            return true;
        }
    }
}
