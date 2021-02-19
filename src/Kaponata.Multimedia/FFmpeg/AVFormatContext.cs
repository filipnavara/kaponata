// <copyright file="AVFormatContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// A context which enables I/O for a given video or audio file format.
    /// </summary>
    public unsafe class AVFormatContext : IDisposable
    {
        private readonly FFmpegClient client;
        private AVFormatContextHandle handle;
        private AVIOContext ioContext;
        private bool diposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContext"/> class.
        /// </summary>
        /// <remarks>
        /// Should only be used for testing purposes.
        /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AVFormatContext()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContext"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="ioContext">
        /// The I/O context which enables reading and writing data using custom callbacks.
        /// </param>
        public AVFormatContext(FFmpegClient client, AVIOContext ioContext)
            : this(client, ioContext, new AVFormatContextHandle(client, client.AllocAVFormatContext()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVFormatContext"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="ioContext">
        /// The I/O context which enables reading and writing data using custom callbacks.
        /// </param>
        /// <param name="formatContextHandle">
        /// The formatContextHandle.
        /// </param>
        public AVFormatContext(FFmpegClient client, AVIOContext ioContext, AVFormatContextHandle formatContextHandle)
        {
            this.client = client;
            this.handle = formatContextHandle;
            this.ioContext = ioContext;
            this.NativeObject->pb = ioContext.NativeObject;
        }

        /// <summary>
        /// Gets the handle to the underlying <see cref="NativeAVFormatContext"/>.
        /// </summary>
        public AVFormatContextHandle Handle => this.handle;

        /// <summary>
        /// Gets a value indicating whether this context has been closed.
        /// </summary>
        public bool IsClosed => this.handle.IsClosed;

        /// <summary>
        /// Gets the native FFmpeg object.
        /// </summary>
        public NativeAVFormatContext* NativeObject => (NativeAVFormatContext*)this.handle.DangerousGetHandle();

        /// <summary>
        /// Gets the I/O context which enables reading and writing data using custom callbacks.
        /// </summary>
        public AVIOContext IOContext => this.ioContext;

        /// <summary>
        /// Gets flags signalling stream properties.
        /// </summary>
        public AVFormatContextFlags Flags => (AVFormatContextFlags)this.NativeObject->ctx_flags;

        /// <summary>
        /// Gets flags for the user to detect events happening on the file. Flags must be cleared
        /// by the user once the event has been handled.
        /// </summary>
        public AVFormatContextEventFlags EventFlags => (AVFormatContextEventFlags)this.NativeObject->event_flags;

        /// <summary>
        /// Gets the number of streams in this format context.
        /// </summary>
        public uint StreamCount => this.NativeObject->nb_streams;

        /// <summary>
        /// Gets the media type of a stream at a specific index.
        /// </summary>
        /// <param name="index">
        /// The index of the stream for which to determine the media type.
        /// </param>
        /// <returns>
        /// The media type for the requested stream.
        /// </returns>
        public NativeAVMediaType GetStreamCodecType(int index)
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

        /// <summary>
        /// Gets the unique video <see cref="AVStream"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AVStream"/> object which represents the requested stream.
        /// </returns>
        public AVStream GetVideoStream()
        {
            if (this.StreamCount > 1)
            {
                throw new InvalidOperationException("There should be only one stream");
            }

            var stream = this.GetStream(0);

            if (stream.CodecParameters.Type != NativeAVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                throw new InvalidOperationException(@"Could not find any video stream.");
            }

            return stream;
        }

        /// <summary>
        /// Open an input stream and read the header.
        /// </summary>
        /// <param name="url">
        /// URL of the stream to open.
        /// </param>
        /// <param name="inputFormat">
        /// If non-NULL, this parameter forces a specific input format. Otherwise the format is autodetected.
        /// </param>
        public void OpenInputStream(string? url, AVInputFormat inputFormat)
        {
            var ret = this.client.avformat_open_input(this, url, inputFormat == null ? null : inputFormat.NativeObject, null);

            this.client.ThrowOnAVError(ret);
        }

        /// <summary>
        /// Open an input stream and read the header.
        /// </summary>
        /// <param name="inputFormat">
        /// The input format.
        /// </param>
        public void OpenInputStream(string inputFormat)
        {
            var h264 = new AVInputFormat(this.client, "h264");
            this.OpenInputStream(null, h264);
        }

        /// <summary>
        /// Close an opened input AVFormatContext.
        /// </summary>
        public void Close()
        {
            this.client.CloseFormatContext(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.diposed)
            {
                return;
            }

            if (this.IOContext != null)
            {
                this.IOContext.Dispose();
            }

            this.handle.Dispose();

            this.diposed = true;
        }
    }
}
