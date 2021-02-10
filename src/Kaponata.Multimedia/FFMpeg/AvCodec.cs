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
        private readonly AVCodecHandle handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodec"/> class.
        /// </summary>
        /// <param name="handle">
        /// A pointer to the unmanaged structure which backs this <see cref="AVCodec"/> class.
        /// </param>
        public AVCodec(AVCodecHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Gets the unmanaged structure which back this <see cref="AVCodec"/> class.
        /// </summary>
        public NativeAVCodec* NativeObject
        {
            get { return (NativeAVCodec*)this.handle.DangerousGetHandle(); }
        }

        /// <summary>
        /// Gets the name of the codec implementation. The name is globally unique among encoders
        /// and among decoders (but an encoder and a decoder can share the same name). This
        /// is the primary way to find a codec from the user perspective.
        /// </summary>
        public string Name
        {
            get { return new string((sbyte*)this.NativeObject->name); }
        }

        /// <summary>
        /// Gets the escriptive name for the codec, meant to be more human readable than name.
        /// </summary>
        public string LongName
        {
            get { return new string((sbyte*)this.NativeObject->long_name); }
        }

        /// <summary>
        /// Gets the ID of this codec.
        /// </summary>
        public AVCodecID Id
        {
            get { return this.NativeObject->id; }
        }

        /// <summary>
        /// Gets the capabilities of this codec.
        /// </summary>
        public AVCodecCapabilities Capabilities
        {
            get { return (AVCodecCapabilities)this.NativeObject->capabilities; }
        }

        /// <summary>
        /// Gets a value indicating whether the codec is an encoder.
        /// </summary>
        public bool IsEncoder
        {
            get { return ffmpeg.av_codec_is_encoder(this.NativeObject) == 1; }
        }

        /// <summary>
        /// Gets a value indicating whether the codec is a decoder.
        /// </summary>
        public bool IsDecoder
        {
            get { return ffmpeg.av_codec_is_decoder(this.NativeObject) == 1; }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.handle.Dispose();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.LongName} ({this.Name})";
        }
    }
}
