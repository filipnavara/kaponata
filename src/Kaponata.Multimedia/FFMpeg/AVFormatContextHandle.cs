// <copyright file="AVFormatContextHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to an <see cref="NativeAVFormatContext"/>.
    /// </summary>
    public class AVFormatContextHandle : SafeHandle
    {
        private readonly FFMpegClient ffmpeg;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContextHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// The ffmpeg client.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        public unsafe AVFormatContextHandle(FFMpegClient ffmpeg, NativeAVFormatContext* context)
        : base((IntPtr)context, true)
        {
            if (ffmpeg == null)
            {
                throw new ArgumentNullException(nameof(ffmpeg));
            }

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
            this.ffmpeg.avformat_free_context((NativeAVFormatContext*)this.handle);
            return true;
        }
    }
}
