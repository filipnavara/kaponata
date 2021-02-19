// <copyright file="AVError.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using NativeFFmpeg = FFmpeg.AutoGen.ffmpeg;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// FFmpeg error codes.
    /// </summary>
    public enum AVError : uint
    {
        /// <summary>
        /// Bitstream filter not found.
        /// </summary>
        BitstreamFilterNotFound = 0xb9acbd08,

        /// <summary>
        /// Buffer too small.
        /// </summary>
        BufferTooSmall = 0xacb9aabe,

        /// <summary>
        /// Internal bug,
        /// </summary>
        Bug = 0xdeb8aabe,

        /// <summary>
        /// This is semantically identical to <see cref="Bug"/>.
        /// </summary>
        Bug2 = 0xdfb8aabe,

        /// <summary>
        /// Decoder not found.
        /// </summary>
        DecoderNotFound = 0xbcbabb08,

        /// <summary>
        /// Demuxer not found.
        /// </summary>
        DemuxerNotFound = 0xb2babb08,

        /// <summary>
        /// Encoder not found.
        /// </summary>
        EncoderNotFound = 0xbcb1ba08,

        /// <summary>
        /// End of file.
        /// </summary>
        EndOfFile = 0xdfb9b0bb,

        /// <summary>
        /// Immediate exit was requested; the called function should not be restarted.
        /// </summary>
        Exit = 0xabb6a7bb,

        /// <summary>
        /// Generic error in an external library.
        /// </summary>
        Experimental = unchecked((uint)-0x2bb2afa8),

        /// <summary>
        /// An external error occurred.
        /// </summary>
        External = 0xdfaba7bb,

        /// <summary>
        /// Filter not found.
        /// </summary>
        FilterNotFound = 0xb3b6b908,

        /// <summary>
        /// A HTTP Bad Request response has been received.
        /// </summary>
        HttpBadRequest = 0xcfcfcb08,

        /// <summary>
        /// A HTTP Forbidden response has been received.
        /// </summary>
        HttpForbidden = 0xcccfcb08,

        /// <summary>
        /// A HTTP Not Found response has been received.
        /// </summary>
        HttpNotFound = 0xcbcfcb08,

        /// <summary>
        /// A HTTP 400 error code has been received.
        /// </summary>
        HttpOther4xx = 0xa7a7cb08,

        /// <summary>
        /// A HTTP Server Error has been received.
        /// </summary>
        HttpServerError = 0xa7a7ca08,

        /// <summary>
        /// A HTTP Unauthorized error has been received.
        /// </summary>
        HttpUnauthorized = 0xcecfcb08,

        /// <summary>
        /// Input changed between calls. Reconfiguration is required.
        /// </summary>
        InputChanged = unchecked((uint)NativeFFmpeg.AVERROR_INPUT_CHANGED),

        /// <summary>
        /// Invalid data found when processing input.
        /// </summary>
        InvalidData = 0xbebbb1b7,

        /// <summary>
        /// Muxer not found.
        /// </summary>
        MuxerNotFound = 0xa7aab208,

        /// <summary>
        /// Option not found.
        /// </summary>
        OptionNotFound = 0xabafb008,

        /// <summary>
        /// Output changed between calls. Reconfiguration is required
        /// </summary>
        OutputChanged = unchecked((uint)NativeFFmpeg.AVERROR_OUTPUT_CHANGED),

        /// <summary>
        /// Patch is welcome.
        /// </summary>
        PatchWelcome = 0xbaa8beb0,

        /// <summary>
        /// Protocol not found.
        /// </summary>
        ProtocolNotFound = 0xb0adaf08,

        /// <summary>
        /// Stream not found.
        /// </summary>
        StreamNotFound = 0xadabac08,

        /// <summary>
        /// An unkown error occurred.
        /// </summary>
        Unknown = 0xb1b4b1ab,
    }
}
