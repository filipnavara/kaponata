// <copyright file="FFmpegClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// for a given codec ID (e.g. H264).
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
    }
}
