// <copyright file="AVCodecHandle.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// A wrapper around a handle (pointer) to an <see cref="NativeAVCodec"/>.
    /// </summary>
    public class AVCodecHandle : SafeHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodecHandle"/> class.
        /// </summary>
        /// <param name="codec">
        /// The handle to be wrapped.
        /// </param>
        public unsafe AVCodecHandle(NativeAVCodec* codec)
            : base((IntPtr)codec, true)
        {
        }

        /// <inheritdoc/>
        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        /// <inheritdoc/>
        protected override unsafe bool ReleaseHandle()
        {
            return true;
        }
    }
}
