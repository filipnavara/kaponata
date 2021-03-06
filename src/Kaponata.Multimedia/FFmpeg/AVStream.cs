// <copyright file="AVStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>
using System.Collections.Generic;
using NativeAVCodecContext = FFmpeg.AutoGen.AVCodecContext;
using NativeAVRational = FFmpeg.AutoGen.AVRational;
using NativeAVStream = FFmpeg.AutoGen.AVStream;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Represents a stream of data.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/3.1/structAVStream.html"/>
    public unsafe class AVStream
    {
        private NativeAVStream* native;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVStream"/> class.
        /// </summary>
        /// <param name="native">
        /// A <see cref="NativeAVStream"/> which points to the underlying native memory.
        /// </param>
        public AVStream(NativeAVStream* native)
        {
            this.native = native;
        }

        /// <summary>
        /// Gets the codec parameters associated with this stream.
        /// </summary>
        public AVCodecParameters CodecParameters => new AVCodecParameters(this.native->codecpar);

        /// <summary>
        /// Gets the codec context.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public NativeAVCodecContext* CodecContext => this.native->codec;
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets the index of this stream.
        /// </summary>
        public int Index => this.native->index;

        /// <summary>
        /// Gets the fundamental unit of time (in seconds) in terms of which frame timestamps
        /// are represented.
        /// </summary>
        public NativeAVRational TimeBase => this.native->time_base;

        /// <summary>
        /// Gets metadata for this stream.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (this.native->metadata == null)
                {
                    return null!;
                }

                return AVDictionaryHelpers.ToReadOnlyDictionary(this.native->metadata);
            }
        }
    }
}
