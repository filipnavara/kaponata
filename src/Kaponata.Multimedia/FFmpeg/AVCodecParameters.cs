// <copyright file="AVCodecParameters.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using NativeAVCodecID = FFmpeg.AutoGen.AVCodecID;
using NativeAVCodecParameters = FFmpeg.AutoGen.AVCodecParameters;
using NativeAVMediaType = FFmpeg.AutoGen.AVMediaType;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// This struct describes the properties of an encoded stream.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/3.1/structAVCodecParameters.html"/>
    public unsafe struct AVCodecParameters
    {
        private NativeAVCodecParameters* native;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVCodecParameters"/> struct.
        /// </summary>
        /// <param name="native">
        /// A <see cref="AVCodecParameters"/> which points to the underlying native memory.
        /// </param>
        public AVCodecParameters(NativeAVCodecParameters* native)
        {
            this.native = native;
        }

        /// <summary>
        /// Gets general type of the encoded data.
        /// </summary>
        public NativeAVMediaType Type => this.native->codec_type;

        /// <summary>
        /// Gets specific type of the encoded data (the codec used).
        /// </summary>
        public NativeAVCodecID Id => this.native->codec_id;

        /// <summary>
        /// Gets the pixel format,
        ///   the value for video corresponds to enum AVPixelFormat.
        ///   the value for audio corresponds to enum AVSampleFormat.
        /// </summary>
        public int Format => this.native->format;
    }
}
