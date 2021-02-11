// <copyright file="AVStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using AVRational = FFmpeg.AutoGen.AVRational;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVStream = FFmpeg.AutoGen.AVStream;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Represents a stream of data.
    /// </summary>
    public unsafe class AVStream : IDisposable
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
        /// Gets the codec context associated with this stream.
        /// </summary>
        public unsafe NativeAVCodecParameters CodecContext
        {
            get
            {
                return *this.native->codecpar;
            }
        }

        /// <summary>
        /// Gets the index of this stream.
        /// </summary>
        public int Index
        {
            get
            {
                return this.native->index;
            }
        }

        /// <summary>
        /// Gets the fundamental unit of time (in seconds) in terms of which frame timestamps
        /// are represented.
        /// </summary>
        public AVRational TimeBase
        {
            get
            {
                return this.native->time_base;
            }
        }

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

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
