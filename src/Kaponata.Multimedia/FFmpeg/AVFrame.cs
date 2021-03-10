// <copyright file="AVFrame.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVFrame = FFmpeg.AutoGen.AVFrame;
using NativeAVPictureType = FFmpeg.AutoGen.AVPictureType;
using NativeAVPixelFormat = FFmpeg.AutoGen.AVPixelFormat;
using NativeIntArray = FFmpeg.AutoGen.int_array8;
using NativePointerArray = FFmpeg.AutoGen.byte_ptrArray8;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// This class describes decoded (raw) audio or video data.
    /// </summary>
    public unsafe class AVFrame : IDisposable
    {
        private readonly AVFrameHandle handle;
        private readonly FFmpegClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFrame"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="handle">
        /// A <see cref="AVFrameHandle"/> which points to the native <see cref="NativeAVFrame"/>
        /// object.</param>
        public AVFrame(FFmpegClient client, AVFrameHandle handle)
        {
            this.handle = handle;
            this.client = client;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFrame"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the native FFmpeg functions.
        /// </param>
        public AVFrame(FFmpegClient client)
            : this(client, new AVFrameHandle(client))
        {
        }

        /// <summary>
        /// Gets the <see cref="AVFrameHandle"/> which points to the native <see cref="NativeAVFrame"/>
        /// object.
        /// </summary>
        public AVFrameHandle Handle => this.handle;

        /// <summary>
        /// Gets the native <see cref="NativeAVFrame"/> object.
        /// </summary>
        public NativeAVFrame* NativeObject => (NativeAVFrame*)this.handle.DangerousGetHandle();

        /// <summary>
        /// Gets the pixel format used by this frame.
        /// </summary>
        public NativeAVPixelFormat Format => (NativeAVPixelFormat)this.NativeObject->format;

        /// <summary>
        /// Gets the type of picture data embedded in this frame.
        /// </summary>
        public NativeAVPictureType PictureType => this.NativeObject->pict_type;

        /// <summary>
        /// Gets a pointer to the picture/channel planes.
        /// </summary>
        public NativePointerArray Data => this.NativeObject->data;

        /// <summary>
        /// Gets, for video, the size in bytes of each picture line. For audio, the size in
        /// bytes of each plane.
        /// </summary>
        public NativeIntArray LineSize => this.NativeObject->linesize;

        /// <summary>
        /// Gets the height of the video frame.
        /// </summary>
        public int Height => this.NativeObject->height;

        /// <summary>
        /// Gets the width of the video frame.
        /// </summary>
        public int Width => this.NativeObject->width;

        /// <summary>
        /// Gets the presentation timestamp in time_base units (time when frame should be shown to user).
        /// </summary>
        public long PresentationTimestamp => this.NativeObject->pts;

        /// <inheritdoc/>
        public void Dispose()
        {
            this.client.UnrefFrame(this);
            this.handle.Dispose();
        }
    }
}
