// <copyright file="AVIOContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Enables FFmpeg to consume custom I/O.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/2.3/structAVIOContext.html"/>
    public unsafe class AVIOContext : IDisposable
    {
        private readonly AVIOContextHandle handle;
        private readonly FFmpegClient ffmpeg;
        private AVMemoryHandle? buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContext"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the
        /// native FFmpeg functions.
        /// </param>
        /// <param name="handle">
        /// A <see cref="AVIOContextHandle"/> which points to the native <see cref="NativeAVIOContext"/>
        /// object.
        /// </param>
        public AVIOContext(FFmpegClient ffmpeg, AVIOContextHandle handle)
        {
            this.ffmpeg = ffmpeg;
            this.handle = handle;
        }

        /// <summary>
        /// Gets a pointer to the native <see cref="NativeAVIOContext"/> object.
        /// </summary>
        public unsafe NativeAVIOContext* NativeObject => (NativeAVIOContext*)this.handle.DangerousGetHandle();

        /// <summary>
        /// Gets a handle to the I/O buffer.
        /// </summary>
        public unsafe AVMemoryHandle Buffer
        {
            get
            {
                if (this.buffer == null)
                {
                   this.buffer = new AVMemoryHandle(this.ffmpeg, this.NativeObject->buffer, ownsHandle: false);
                }

                return this.buffer;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.handle.Dispose();
            this.Buffer.Dispose();
        }
    }
}
