// <copyright file="AvCodec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;
using AVCodecID = FFmpeg.AutoGen.AVCodecID;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Represents a coder or decoder which can encode or decode audio and video files.
    /// </summary>
    public unsafe class AVCodec : IDisposable
    {
        private readonly NativeAVCodec* native;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodec"/> class.
        /// </summary>
        /// <param name="native">
        /// A pointer to the unmanaged structure which backs this <see cref="AVCodec"/> class.
        /// </param>
        public AVCodec(NativeAVCodec* native)
        {
            this.native = native;
        }

        /// <summary>
        /// Gets the name of the codec implementation. The name is globally unique among encoders
        /// and among decoders (but an encoder and a decoder can share the same name). This
        /// is the primary way to find a codec from the user perspective.
        /// </summary>
        public string Name
        {
            get { return new string((sbyte*)this.native->name); }
        }

        /// <summary>
        /// Gets the escriptive name for the codec, meant to be more human readable than name.
        /// </summary>
        public string LongName
        {
            get { return new string((sbyte*)this.native->long_name); }
        }

        /// <summary>
        /// Gets the ID of this codec.
        /// </summary>
        public AVCodecID Id
        {
            get { return this.native->id; }
        }

        /// <summary>
        /// Gets the capabilities of this codec.
        /// </summary>
        public AVCodecCapabilities Capabilities
        {
            get { return (AVCodecCapabilities)this.native->capabilities; }
        }

        /// <summary>
        /// Gets a value indicating whether the codec is an encoder.
        /// </summary>
        public bool IsEncoder
        {
            get { return ffmpeg.av_codec_is_encoder(this.native) == 1; }
        }

        /// <summary>
        /// Gets a value indicating whether the codec is a decoder.
        /// </summary>
        public bool IsDecoder
        {
            get { return ffmpeg.av_codec_is_decoder(this.native) == 1; }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.LongName} ({this.Name})";
        }
    }
}
