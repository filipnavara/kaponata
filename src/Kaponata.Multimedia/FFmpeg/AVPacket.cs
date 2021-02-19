// <copyright file="AVPacket.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using NativeAVPacket = FFmpeg.AutoGen.AVPacket;
using NativeFFmpeg = FFmpeg.AutoGen.ffmpeg;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Stores compressed data. It is typically exported by demuxers and
    /// then passed as input to decoders, or received as output from encoders and then
    /// passed to muxers.
    /// </summary>
    public unsafe class AVPacket : IDisposable
    {
        private readonly IntPtr handle;
        private readonly FFmpegClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVPacket"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> interface which provides access to the native FFmpeg functions.
        /// </param>
        public AVPacket(FFmpegClient client)
        {
            this.handle = Marshal.AllocHGlobal(Marshal.SizeOf<NativeAVPacket>());
            this.client = client;
            client.InitPacket(this.handle);
        }

        /// <summary>
        /// Gets a handle to the unmanaged memory.
        /// </summary>
        public IntPtr Handle => this.handle;

        /// <summary>
        /// Gets a pointer to the native <see cref="NativeAVPacket"/> object.
        /// </summary>
        public NativeAVPacket* NativeObject => (NativeAVPacket*)this.handle;

        /// <summary>
        /// Gets the index of the stream to which this packet belongs.
        /// </summary>
        public int StreamIndex => this.NativeObject->stream_index;

        /// <summary>
        /// Gets the size of this packet.
        /// </summary>
        public int Size => this.NativeObject->size;

        /// <summary>
        /// Gets the decompression timestamp of this packet in <see cref="AVStream.TimeBase"/> units.
        /// </summary>
        public long DecompressionTimestamp => this.NativeObject->dts;

        /// <summary>
        /// Gets the duration of this packet in <see cref="AVStream.TimeBase"/> units, or 0 if unknown.
        /// </summary>
        public long Duration => this.NativeObject->duration;

        /// <summary>
        /// Gets a combination of <see cref="AVPacketFlags"/> values which describe this packet.
        /// </summary>
        public AVPacketFlags Flags => (AVPacketFlags)this.NativeObject->flags;

        /// <summary>
        /// Gets the byte position in the stream, or -1 if unknown.
        /// </summary>
        public long Position => this.NativeObject->pos;

        /// <summary>
        /// Gets the presentation timestamp of this packet in <see cref="AVStream.TimeBase"/> units.
        /// </summary>
        public long PresentationTimestamp => this.NativeObject->pts;

        /// <summary>
        /// Gets the native packet data.
        /// </summary>
        public unsafe byte* Data => this.NativeObject->data;

        /// <summary>
        /// Return the next frame of a stream.
        /// </summary>
        /// <param name="formatContext">
        /// The format context.
        /// </param>
        /// <returns>
        /// Value indicating wheter a frame was received.
        /// </returns>
        public bool ReadFrame(AVFormatContext formatContext)
        {
            int ret = this.client.ReadFrame(formatContext, this);

            if (ret == 0)
            {
                return true;
            }
            else if (ret == NativeFFmpeg.AVERROR_EOF)
            {
                return false;
            }
            else
            {
                this.client.ThrowOnAVError(ret, postiveIndicatesSuccess: false);
                return false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.client.UnrefPacket(this);
            Marshal.FreeHGlobal(this.handle);
        }
    }
}
