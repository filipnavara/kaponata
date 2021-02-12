// <copyright file="AVIOContextHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to an <see cref="NativeAVIOContext"/>.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/2.3/structAVIOContext.html"/>
    public class AVIOContextHandle : SafeHandle
    {
        private readonly FFMpegClient ffmpeg;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContextHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFMpegClient"/> interface, which allows releasing
        /// the handle.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        public unsafe AVIOContextHandle(FFMpegClient ffmpeg, NativeAVIOContext* context)
            : this(ffmpeg, context, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContextHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFMpegClient"/> interface, which allows releasing
        /// the handle.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        /// <param name="ownsHandle">
        /// <see langword="true"/> to reliably let <see cref="AVIOContextHandle"/> release the handle
        /// during the finalization phase; otherwise, <see langword="false "/> (not recommended).
        /// </param>
        public unsafe AVIOContextHandle(FFMpegClient ffmpeg, NativeAVIOContext* context, bool ownsHandle)
            : base((IntPtr)context, ownsHandle)
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
            this.ffmpeg.FreeAVHandle(this);
            return true;
        }
    }
}
