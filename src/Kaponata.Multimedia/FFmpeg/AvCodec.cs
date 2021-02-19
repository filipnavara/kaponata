// <copyright file="AvCodec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;
using NativeAVCodecContext = FFmpeg.AutoGen.AVCodecContext;
using NativeAVCodecID = FFmpeg.AutoGen.AVCodecID;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Represents a coder or decoder which can encode or decode audio and video files.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/2.5/structAVCodec.html"/>
    public unsafe class AVCodec : IDisposable
    {
        private readonly NativeAVCodec* native;
        private readonly FFmpegClient client;
        private readonly NativeAVCodecContext* context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodec"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="stream">
        /// The identifier of the requested codec.
        /// </param>
        public AVCodec(FFmpegClient client, AVStream stream)
        {
            this.context = stream.CodecContext;
            this.native = (NativeAVCodec*)client.FindDecoder(context->codec_id);

            this.client = client;

            if ((this.Capabilities & AVCodecCapabilities.Truncated) == AVCodecCapabilities.Truncated)
            {
                context->flags |= (int)AVCodecCapabilities.Truncated;
            }

            this.Open(this.context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodec"/> class.
        /// </summary>
        /// <param name="client">
        /// An implementation of the <see cref="FFmpegClient"/> which provides access to the native FFmpeg functions.
        /// </param>
        /// <param name="context">
        /// The the native codec context.
        /// </param>
        /// <param name="codec">
        /// The netive codec object.
        /// </param>
        /// <remarks>
        /// Use only for testing purposes.
        /// </remarks>
        public AVCodec(FFmpegClient client, NativeAVCodecContext* context, NativeAVCodec* codec)
        {
            this.context = context;
            this.native = codec;
            this.client = client;
        }

        /// <summary>
        /// Gets the name of the codec implementation. The name is globally unique among encoders
        /// and among decoders (but an encoder and a decoder can share the same name). This
        /// is the primary way to find a codec from the user perspective.
        /// </summary>
        public string Name => new string((sbyte*)this.native->name);

        /// <summary>
        /// Gets the escriptive name for the codec, meant to be more human readable than name.
        /// </summary>
        public string LongName => new string((sbyte*)this.native->long_name);

        /// <summary>
        /// Gets the ID of this codec.
        /// </summary>
        public NativeAVCodecID Id => this.native->id;

        /// <summary>
        /// Gets the capabilities of this codec.
        /// </summary>
        public AVCodecCapabilities Capabilities => (AVCodecCapabilities)this.native->capabilities;

        /// <summary>
        /// Gets a value indicating whether the codec is an encoder.
        /// </summary>
        public bool IsEncoder => this.client.IsEncoder(this.native);

        /// <summary>
        /// Gets a value indicating whether the codec is a decoder.
        /// </summary>
        public bool IsDecoder => this.client.IsDecoder((IntPtr)this.native);

        /// <summary>
        /// Initialize the AVCodecContext to use the given AVCodec.
        /// </summary>
        /// <param name="context">
        /// The context to initialize.
        /// </param>
        public void Open(NativeAVCodecContext* context)
        {
            var ret = this.client.OpenCodec(context, this.native, null);
            this.client.ThrowOnAVError(ret);
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        public void SendPacket(AVPacket packet)
        {
            int ret = 0;

            if (!this.client.IsCodecOpen((IntPtr)this.context))
            {
                throw new InvalidOperationException("You must first open the codec for this context, via avcodec_open2.");
            }

            if (!this.client.IsDecoder((IntPtr)this.native))
            {
                throw new InvalidOperationException("The codec for this context is not a decoder. You cannot send packets.");
            }

            ret = this.client.SendPacket((IntPtr)this.context, (IntPtr)packet.NativeObject);

            this.client.ThrowOnAVError(ret);
        }

        /// <summary>
        /// Receives decoded output data from a decoder.
        /// </summary>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <returns>
        /// A value indicating wheter a frame was received.
        /// </returns>
        public bool ReceiveFrame(AVFrame frame)
        {
            var ret = this.client.ReceiveFrame(this.context, frame);

            if ((UnixError)(-ret) == UnixError.EAGAIN || (AVError)ret == AVError.EndOfFile)
            {
                return false;
            }

            this.client.ThrowOnAVError(ret);
            return true;
        }

        /// <summary>
        /// Close a given AVCodecContext and free all the data associated with it (but not the AVCodecContext itself).
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void Close(NativeAVCodecContext* context)
        {
            this.client.CloseCodec(context);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.LongName} ({this.Name})";
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Close(this.context);
        }
    }
}
