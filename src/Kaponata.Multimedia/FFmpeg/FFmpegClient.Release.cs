// <copyright file="FFmpegClient.Release.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Contains methods to free native ffmpeg objects. This interface
    /// is meant for consumption by the various safe handles only.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/trunk/group__lavf__core.html"/>
    /// <see href="https://ffmpeg.org/doxygen/2.4/group__lavu__mem.html"/>
    public unsafe partial class FFmpegClient
    {
        /// <summary>
        /// Free an <see cref="NativeAVFormatContext"/> and all its streams.
        /// </summary>
        /// <param name="handle">
        /// context to free.
        /// </param>
        public virtual void FreeAVFormatContext(IntPtr handle)
        {
            ffmpeg.avformat_free_context((NativeAVFormatContext*)handle);
        }

        /// <summary>
        /// Free a memory block which has been allocated with av_malloc(z)() or av_realloc().
        /// </summary>
        /// <param name="handle">
        /// Pointer to the memory block which should be freed.
        /// </param>
        public virtual void FreeAVHandle(SafeHandle handle)
        {
            ffmpeg.av_free(handle.DangerousGetHandle().ToPointer());
        }
    }
}
