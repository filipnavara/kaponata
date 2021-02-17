// <copyright file="FFmpegClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;
using NativeAVCodecDescriptor = FFmpeg.AutoGen.AVCodecDescriptor;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Implements FFmpeg methods.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element must begin with upper-case letter", Justification = "Native function names.")]
    public partial class FFmpegClient
    {
        /// <summary>
        /// Lists the available codecs IDs. There can be multiple codecs (e.g. for decoding or encoding, or with and without hardware acceleration)
        /// for a given codec ID (e.g. H264). Use <see cref="GetAvailableCodecs"/> to get an extensive list.
        /// </summary>
        /// <returns>
        /// A list of available codecs IDs.
        /// </returns>
        public unsafe List<AVCodecDescriptor> GetAvailableCodecDescriptors()
        {
            List<AVCodecDescriptor> codecs = new List<AVCodecDescriptor>();
            NativeAVCodecDescriptor* descriptor = null;

            while ((descriptor = ffmpeg.avcodec_descriptor_next(descriptor)) != null)
            {
                codecs.Add(new AVCodecDescriptor(descriptor));
            }

            return codecs;
        }

        /// <summary>
        /// Lists all available codecs. This list can include multiple codecs for the same codec ID.
        /// </summary>
        /// <returns>
        /// A list of all available codecs.
        /// </returns>
        public unsafe List<AVCodec> GetAvailableCodecs()
        {
            var codecs = new List<AVCodec>();

            void* iter = null;
            NativeAVCodec* codec;

            while ((codec = ffmpeg.av_codec_iterate(&iter)) != null)
            {
                codecs.Add(new AVCodec(codec));
            }

            return codecs;
        }

        /// <summary>
        /// Find a registered decoder with a matching codec ID.
        /// </summary>
        /// <param name="id">
        /// AVCodecID of the requested decoder.
        /// </param>
        /// <returns>
        /// The <see cref="AVCodec"/> mathcing the <see cref="AVCodecID"/>.
        /// </returns>
        public unsafe AVCodec avcodec_find_decoder(AVCodecID id)
        {
            return new AVCodec(ffmpeg.avcodec_find_decoder(id));
        }
    }
}
