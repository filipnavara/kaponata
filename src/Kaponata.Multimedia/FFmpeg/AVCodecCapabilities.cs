// <copyright file="AVCodecCapabilities.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Represents the capabilities of an codec.
    /// </summary>
    /// <seealso href="http://ffmpeg.org/doxygen/trunk/codec_8h.html"/>
    [Flags]
    public enum AVCodecCapabilities : uint
    {
        /// <summary>
        /// No capabilities are defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Codec supports avctx->thread_count == 0
        /// </summary>
        AutoThreads = ffmpeg.AV_CODEC_CAP_AUTO_THREADS,

        /// <summary>
        /// Codec avoids probing.
        /// </summary>
        AvoidProbing = ffmpeg.AV_CODEC_CAP_AVOID_PROBING,

        /// <summary>
        /// Codec should fill in channel configuration and samplerate instead of container.
        /// </summary>
        ChannelConf = ffmpeg.AV_CODEC_CAP_CHANNEL_CONF,

        /// <summary>
        /// Encoder or decoder requires flushing with NULL input at the end in order to give the complete and correct output.
        /// NOTE: If this flag is not set, the codec is guaranteed to never be fed with with NULL data. The user can still send NULL data to the public encode or decode function, but libavcodec will not pass it along to the codec unless this flag is set.
        /// Decoders: The decoder has a non-zero delay and needs to be fed with avpkt->data= NULL, avpkt->size= 0 at the end to get the delayed data until the decoder no longer returns frames.
        /// Encoders: The encoder needs to be fed with NULL data at the end of encoding until the encoder no longer returns data.
        /// NOTE: For encoders implementing the AVCodec.encode2() function, setting this flag also means that the encoder must set the pts and duration for each output packet.If this flag is not set, the pts and duration will be determined by libavcodec from the input frame.
        /// </summary>
        Delay = ffmpeg.AV_CODEC_CAP_DELAY,

        /// <summary>
        /// Codec uses get_buffer() for allocating buffers and supports custom allocators.
        /// </summary>
        DR1 = ffmpeg.AV_CODEC_CAP_DR1,

        /// <summary>
        /// Decoder can use draw_horiz_band callback.
        /// </summary>
        DrawHorizontalBand = ffmpeg.AV_CODEC_CAP_DRAW_HORIZ_BAND,

        /// <summary>
        /// Codec is experimental and is thus avoided in favor of non experimental encoders.
        /// </summary>
        Experimental = ffmpeg.AV_CODEC_CAP_EXPERIMENTAL,

        /// <summary>
        /// Codec supports frame-level multithreading.
        /// </summary>
        Threads = ffmpeg.AV_CODEC_CAP_FRAME_THREADS,

        /// <summary>
        /// Codec is intra only.
        /// </summary>
        IntraOnly = ffmpeg.AV_CODEC_CAP_INTRA_ONLY,

        /// <summary>
        /// Codec is lossless.
        /// </summary>
        Losless = ffmpeg.AV_CODEC_CAP_LOSSLESS,

        /// <summary>
        /// Codec supports changed parameters at any point.
        /// </summary>
        ParamChange = ffmpeg.AV_CODEC_CAP_PARAM_CHANGE,

        /// <summary>
        /// Codec supports slice-based (or partition-based) multithreading.
        /// </summary>
        SliceThreads = ffmpeg.AV_CODEC_CAP_SLICE_THREADS,

        /// <summary>
        /// Codec can be fed a final frame with a smaller size.
        /// </summary>
        SmallLastFrame = ffmpeg.AV_CODEC_CAP_SMALL_LAST_FRAME,

        /// <summary>
        /// Codec can output multiple frames per AVPacket Normally demuxers return one frame at a time,
        /// demuxers which do not do are connected to a parser to split what they return into proper frames.
        /// </summary>
        Subframes = ffmpeg.AV_CODEC_CAP_SUBFRAMES,

        /// <summary>
        /// Codec supports truncated frames.
        /// </summary>
        Truncated = ffmpeg.AV_CODEC_CAP_TRUNCATED,

        /// <summary>
        /// Audio encoder supports receiving a different number of samples in each call.
        /// </summary>
        VariableFrameSize = ffmpeg.AV_CODEC_CAP_VARIABLE_FRAME_SIZE,
    }
}
