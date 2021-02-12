// <copyright file="AVMemoryHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to a general-purpose memory allocated by FFmpeg.
    /// </summary>
    public class AVMemoryHandle : SafeHandle
    {
        private readonly FFMpegClient ffmpeg;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVMemoryHandle"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFMpegClient"/> interface, which allows releasing
        /// the handle.
        /// </param>
        /// <param name="context">
        /// The handle to be wrapped.
        /// </param>
        /// <param name="ownsHandle">
        /// <see langword="true"/> to reliably let <see cref="AVMemoryHandle"/> release the handle
        /// during the finalization phase; otherwise, <see langword="false "/> (not recommended).
        /// </param>
        public unsafe AVMemoryHandle(FFMpegClient ffmpeg, void* context, bool ownsHandle)
            : base((IntPtr)context, ownsHandle)
        {
            this.ffmpeg = ffmpeg;
        }

        /// <inheritdoc/>
        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Gets a direct pointer to the unmanaged memory.
        /// </summary>
        public unsafe byte* NativeObject
        {
            get { return (byte*)this.handle; }
        }

        /// <inheritdoc/>
        protected override unsafe bool ReleaseHandle()
        {
            this.ffmpeg.FreeAVHandle(this);
            return true;
        }
    }
}
