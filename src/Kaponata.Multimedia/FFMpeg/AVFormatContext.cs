// <copyright file="AVFormatContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using AVMediaType = FFmpeg.AutoGen.AVMediaType;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// A context which enables I/O for a given video or audio file format.
    /// </summary>
    public unsafe class AVFormatContext : IDisposable
    {
        private readonly FFMpegClient client;

        private AVFormatContextHandle handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContext"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFMpegClient"/> which provides access to the native FFmpeg
        /// functions.
        /// </param>
        /// <param name="handle">
        /// A handle to the unmanaged AV format context.
        /// </param>
        public AVFormatContext(FFMpegClient client, AVFormatContextHandle handle)
        {
            this.handle = handle;
            this.client = client;
        }

        /// <summary>
        /// Gets the handle to the underlying <see cref="NativeAVFormatContext"/>.
        /// </summary>
        public AVFormatContextHandle Handle
        {
            get { return this.handle; }
        }

        /// <summary>
        /// Gets a value indicating whether this context has been closed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.handle.IsClosed; }
        }

        /// <summary>
        /// Gets or sets the native FFmpeg object.
        /// </summary>
        public NativeAVFormatContext* NativeObject
        {
            get
            {
                return (NativeAVFormatContext*)this.handle.DangerousGetHandle();
            }

            set
            {
                // Mark the original handle as invalid - NativeObject is updated when native code has changed
                // the handle.
                this.handle.SetHandleAsInvalid();
                this.handle = new AVFormatContextHandle(this.client, value);
            }
        }

        /// <summary>
        /// Gets or sets the I/O context which enables reading and writing data using custom callbacks.
        /// </summary>
        public AVIOContext IOContext
        {
            get
            {
                return new AVIOContext(this.client, new AVIOContextHandle(this.client, this.NativeObject->pb, ownsHandle: false));
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.NativeObject->pb = value.NativeObject;
            }
        }

        /// <summary>
        /// Gets flags signalling stream properties.
        /// </summary>
        public AVFormatContextFlags Flags
        {
            get { return (AVFormatContextFlags)this.NativeObject->ctx_flags; }
        }

        /// <summary>
        /// Gets flags for the user to detect events happening on the file. Flags must be cleared
        /// by the user once the event has been handled.
        /// </summary>
        public AVFormatContextEventFlags EventFlags
        {
            get { return (AVFormatContextEventFlags)this.NativeObject->event_flags; }
        }

        /// <summary>
        /// Gets the number of streams in this format context.
        /// </summary>
        public uint StreamCount
        {
            get { return this.NativeObject->nb_streams; }
        }

        /// <summary>
        /// Gets the media type of a stream at a specific index.
        /// </summary>
        /// <param name="index">
        /// The index of the stream for which to determine the media type.
        /// </param>
        /// <returns>
        /// The media type for the requested stream.
        /// </returns>
        public FFmpeg.AutoGen.AVMediaType GetStreamCodecType(int index)
        {
            if (index < 0 || index >= this.StreamCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var stream = this.NativeObject->streams[index];
            return stream->codecpar->codec_type;
        }

        /// <summary>
        /// Gets the <see cref="AVStream"/> at a specific index.
        /// </summary>
        /// <param name="index">
        /// The index of the stream to retrieve.
        /// </param>
        /// <returns>
        /// An <see cref="AVStream"/> object which represents the requested stream.
        /// </returns>
        public AVStream GetStream(int index)
        {
            if (index < 0 || index >= this.StreamCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new AVStream(this.NativeObject->streams[index]);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.handle.Dispose();
        }
    }
}
