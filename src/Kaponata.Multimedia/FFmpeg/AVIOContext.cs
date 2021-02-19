// <copyright file="AVIOContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;
using NativeReadPacketFunc = FFmpeg.AutoGen.avio_alloc_context_read_packet_func;
using NativeWritePacketFunc = FFmpeg.AutoGen.avio_alloc_context_write_packet_func;

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
        private AVMemoryHandle buffer;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContext"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="handle">
        /// A <see cref="AVIOContextHandle"/> which points to the native <see cref="NativeAVIOContext"/>
        /// object.
        /// </param>
        public AVIOContext(FFmpegClient ffmpeg, AVIOContextHandle handle)
        {
            this.ffmpeg = ffmpeg;
            this.handle = handle;
            this.buffer = new AVMemoryHandle(this.ffmpeg, this.NativeObject->buffer, ownsHandle: false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIOContext"/> class.
        /// </summary>
        /// <param name="ffmpeg">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the
        /// native FFmpeg functions.
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size.
        /// </param>
        /// <param name="read_packet">
        /// A function for refilling the buffer, may be NULL. For stream protocols, must never return 0 but rather a proper AVERROR code.
        /// </param>
        /// <param name="write_packet">
        /// A function for writing the buffer contents, may be NULL. The function may not change the input buffers content.
        /// </param>
        public AVIOContext(FFmpegClient ffmpeg, ulong bufferSize, NativeReadPacketFunc read_packet, NativeWritePacketFunc? write_packet)
        {
            this.ffmpeg = ffmpeg;

            void* memory = ffmpeg.AllocMemory(bufferSize);

            var memoryHandle = new AVMemoryHandle(ffmpeg, memory, false);

            var avio_ctx = ffmpeg.AllocAVIOContext((byte*)memory, (int)bufferSize, write_packet == null ? 0 : 1, (void*)IntPtr.Zero, read_packet, write_packet.GetValueOrDefault(), null);
            this.buffer = memoryHandle;

            this.handle = new AVIOContextHandle(ffmpeg, avio_ctx);
        }

        /// <summary>
        /// Gets a pointer to the native <see cref="NativeAVIOContext"/> object.
        /// </summary>
        public unsafe NativeAVIOContext* NativeObject => (NativeAVIOContext*)this.handle.DangerousGetHandle();

        /// <summary>
        /// Gets a handle to the I/O buffer.
        /// </summary>
        public unsafe AVMemoryHandle Buffer => this.buffer;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (!this.disposed)
            {
                this.handle.Dispose();
                this.Buffer.Dispose();
                this.disposed = true;
            }
        }
    }
}
