// <copyright file="FFmpegClient.Release.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;
using NativeAVCodec = FFmpeg.AutoGen.AVCodec;
using NativeAVDictionary = FFmpeg.AutoGen.AVDictionary;
using NativeAVFormatContext = FFmpeg.AutoGen.AVFormatContext;
using NativeAVFrame = FFmpeg.AutoGen.AVFrame;
using NativeAVInputFormat = FFmpeg.AutoGen.AVInputFormat;
using NativeAVIOContext = FFmpeg.AutoGen.AVIOContext;
using NativeAVPacket = FFmpeg.AutoGen.AVPacket;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Contains methods to free native ffmpeg objects. This interface
    /// is meant for consumption by the various safe handles only.
    /// </summary>
    /// <seealso href="https://ffmpeg.org/doxygen/trunk/group__lavf__core.html"/>
    /// <see href="https://ffmpeg.org/doxygen/2.4/group__lavu__mem.html"/>
    public unsafe partial class FFmpegClient
    {
        /// <summary>
        /// Free an <see cref="NativeAVFormatContext"/> and all its streams.
        /// </summary>
        /// <param name="handle">
        /// context to free.
        /// </param>
        public virtual void FreeAVFormatContext(IntPtr handle)
        {
            ffmpeg.avformat_free_context((NativeAVFormatContext*)handle);
        }

        /// <summary>
        /// Free a memory block which has been allocated with av_malloc(z)() or av_realloc().
        /// </summary>
        /// <param name="handle">
        /// Pointer to the memory block which should be freed.
        /// </param>
        public virtual void FreeAVHandle(SafeHandle handle)
        {
            ffmpeg.av_free(handle.DangerousGetHandle().ToPointer());
        }

        /// <summary>
        /// Find AVInputFormat based on the short name of the input format.
        /// </summary>
        /// <param name="shortName">
        /// The short name of the AVInputFormat.
        /// </param>
        /// <returns>
        /// A pointer pointing to the native AVInputFormat object.
        /// </returns>
        public virtual IntPtr FindInputFormat(string shortName)
        {
            return new IntPtr(ffmpeg.av_find_input_format(shortName));
        }

        /// <summary>
        /// Find a registered decoder with a matching codec ID.
        /// </summary>
        /// <param name="codecId">
        /// AVCodecID of the requested decoder.
        /// </param>
        /// <returns>
        /// A decoder if one was found, NULL otherwise.
        /// </returns>
        public virtual IntPtr FindDecoder(AVCodecID codecId)
        {
            return (IntPtr)ffmpeg.avcodec_find_decoder(codecId);
        }

        /// <summary>
        /// Allocate an AVFormatContext. avformat_free_context() can be used to free the context and everything allocated by the framework within it.
        /// </summary>
        /// <returns>
        /// A pointer to the native AVFormatContext object.
        /// </returns>
        public virtual NativeAVFormatContext* AllocAVFormatContext()
        {
            return ffmpeg.avformat_alloc_context();
        }

        /// <summary>
        /// Open an input stream and read the header. The codecs are not opened. The stream must be closed with avformat_close_input().
        /// </summary>
        /// <param name="context">
        /// Pointer to user-supplied AVFormatContext (allocated by avformat_alloc_context).
        /// May be a pointer to NULL, in which case an AVFormatContext is allocated by this
        /// function and written into ps. Note that a user-supplied AVFormatContext will
        /// be freed on failure.
        /// </param>
        /// <param name="url">
        /// URL of the stream to open.
        /// </param>
        /// <param name="fmt">
        /// If non-NULL, this parameter forces a specific input format. Otherwise the format is autodetected.
        /// </param>
        /// <param name="options">
        /// A dictionary filled with AVFormatContext and demuxer-private options.On return
        /// this parameter will be destroyed and replaced with a dict containing options
        /// that were not found. May be NULL.
        /// </param>
        /// <returns>
        /// 0 on success, a negative AVERROR on failure.
        /// </returns>
        public int avformat_open_input(AVFormatContext context, string? url, NativeAVInputFormat* fmt, AVDictionary** options)
        {
            var ctx = context.NativeObject;
            return ffmpeg.avformat_open_input(&ctx, url, fmt, options);
        }

        /// <summary>
        /// Allocate and initialize an AVIOContext for buffered I/O. It must be later freed with avio_context_free().
        /// </summary>
        /// <param name="buffer">
        /// Memory block for input/output operations via AVIOContext. The buffer must be allocated with av_malloc() and friends.
        /// It may be freed and replaced with a new buffer by libavformat. AVIOContext.buffer holds the buffer currently in use, which must be later freed with av_free().
        /// </param>
        /// <param name="buffer_size">
        /// The buffer size is very important for performance. For protocols with fixed blocksize it should be set to this blocksize. For others a typical size is a cache page, e.g. 4kb.
        /// </param>
        /// <param name="write_flag">
        /// Set to 1 if the buffer should be writable, 0 otherwise.
        /// </param>
        /// <param name="opaque">
        /// An opaque pointer to user-specific data.
        /// </param>
        /// <param name="read_packet">
        /// A function for refilling the buffer, may be NULL. For stream protocols, must never return 0 but rather a proper AVERROR code.
        /// </param>
        /// <param name="write_packet">
        /// A function for writing the buffer contents, may be NULL. The function may not change the input buffers content.
        /// </param>
        /// <param name="seek">
        /// A function for seeking to specified byte position, may be NULL.
        /// </param>
        /// <returns>
        /// Allocated AVIOContext.
        /// </returns>
        public NativeAVIOContext* AllocAVIOContext(byte* buffer, int buffer_size, int write_flag, void* opaque, avio_alloc_context_read_packet_func read_packet, avio_alloc_context_write_packet_func write_packet, avio_alloc_context_seek_func seek)
        {
            var avio_ctx = ffmpeg.avio_alloc_context(buffer, buffer_size, write_flag, opaque, read_packet, write_packet, seek);
            if (avio_ctx == null)
            {
                throw new OutOfMemoryException();
            }

            return avio_ctx;
        }

        /// <summary>
        /// Allocate a memory block with alignment suitable for all memory accesses (including vectors if available on the CPU).
        /// </summary>
        /// <param name="size">
        /// Size in bytes for the memory block to be allocated.
        /// </param>
        /// <returns>
        /// Pointer to the allocated block.
        /// </returns>
        public void* AllocMemory(ulong size)
        {
            var memory = ffmpeg.av_malloc(size);
            if (memory == null)
            {
                throw new OutOfMemoryException();
            }

            return memory;
        }

        /// <summary>
        /// Allocate an AVFrame and set its fields to default values. The resulting struct must be freed using av_frame_free().
        /// </summary>
        /// <returns>
        /// An AVFrame filled with default values or NULL on failure.
        /// </returns>
        public virtual IntPtr AllocFrame()
        {
            return new IntPtr(ffmpeg.av_frame_alloc());
        }

        /// <summary>
        /// Free the frame and any dynamically allocated objects in it, e.g. extended_data. If the frame is reference counted, it will be unreferenced first.
        /// </summary>
        /// <param name="frameHandle">
        /// The frame to be freed. The pointer will be set to NULL.
        /// </param>
        public virtual void FreeFrame(IntPtr frameHandle)
        {
            ffmpeg.av_frame_free((NativeAVFrame**)&frameHandle);
        }

        /// <summary>
        /// Initialize optional fields of a packet with default values.
        /// </summary>
        /// <param name="handle">
        /// The packet handle.
        /// </param>
        public virtual void InitPacket(IntPtr handle)
        {
            ffmpeg.av_init_packet((NativeAVPacket*)handle);
        }

        /// <summary>
        /// Allocate an AVPacket and set its fields to default values. The resulting struct must be freed using av_packet_free().
        /// </summary>
        /// <returns>
        /// An AVPacket filled with default values or NULL on failure.
        /// </returns>
        public virtual IntPtr AllocPacket()
        {
            return (IntPtr)ffmpeg.av_packet_alloc();
        }

        /// <summary>
        /// Free the packet, if the packet is reference counted, it will be unreferenced first.
        /// </summary>
        /// <param name="packet">
        /// packet to be freed. The pointer will be set to NULL.
        /// </param>
        public virtual void FreePacket(IntPtr packet)
        {
            ffmpeg.av_packet_free((NativeAVPacket**)&packet);
        }

        /// <summary>
        /// Supply raw packet data as input to a decoder.
        /// </summary>
        /// <param name="codecContext">
        /// The codec context.
        /// </param>
        /// <param name="packet">
        /// The input AVPacket.
        /// </param>
        /// <returns>
        /// 0 on success, otherwise negative error code: AVERROR(EAGAIN).
        /// </returns>
        public virtual int SendPacket(IntPtr codecContext, IntPtr packet)
        {
            return ffmpeg.avcodec_send_packet((AVCodecContext*)codecContext, (NativeAVPacket*)packet);
        }

        /// <summary>
        /// Returns a value indicating whether the codec context is open.
        /// </summary>
        /// <param name="codecContext">
        /// The codec context.
        /// </param>
        /// <returns>
        /// A value indicating whether the codec is open.
        /// </returns>
        public virtual bool IsCodecOpen(IntPtr codecContext)
        {
            return ffmpeg.avcodec_is_open((AVCodecContext*)codecContext) > 0;
        }

        /// <summary>
        /// Returns a value indicating whether the codec is a decoder.
        /// </summary>
        /// <param name="codec">
        /// The codec.
        /// </param>
        /// <returns>
        /// Avalue indicating whether the codec is a decoder.
        /// </returns>
        public virtual bool IsDecoder(IntPtr codec)
        {
            return ffmpeg.av_codec_is_decoder((NativeAVCodec*)codec) > 0;
        }

        /// <summary>
        /// Returns a value indicating whether the codec is an encoder.
        /// </summary>
        /// <param name="codec">
        /// The codec.
        /// </param>
        /// <returns>
        /// Avalue indicating whether the codec is an encoder.
        /// </returns>
        public bool IsEncoder(NativeAVCodec* codec)
        {
            return ffmpeg.av_codec_is_encoder(codec) > 0;
        }

        /// <summary>
        /// Initialize the AVCodecContext to use the given AVCodec.
        /// </summary>
        /// <param name="codecContext">
        /// The context to initialize.
        /// </param>
        /// <param name="codec">
        /// The codec to open this context for.
        /// </param>
        /// <param name="options">
        /// A dictionary filled with AVCodecContext and codec-private options.
        /// </param>
        /// <returns>
        /// zero on success, a negative value on error.
        /// </returns>
        public int OpenCodec(AVCodecContext* codecContext, NativeAVCodec* codec, NativeAVDictionary** options)
        {
            return ffmpeg.avcodec_open2(codecContext, codec, options);
        }

        /// <summary>
        /// Return decoded output data from a decoder.
        /// </summary>
        /// <param name="context">
        /// The codec context.
        /// </param>
        /// <param name="frame">
        /// This will be set to a reference-counted video or audio frame.
        /// </param>
        /// <returns>
        /// 0: success, a frame was returned AVERROR(EAGAIN): output is not available in this state.
        /// </returns>
        public int ReceiveFrame(AVCodecContext* context, AVFrame frame)
        {
            return ffmpeg.avcodec_receive_frame(context, frame.NativeObject);
        }

        /// <summary>
        /// Return the next frame of a stream.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="packet">
        /// The packet.
        /// </param>
        /// <returns>
        /// 0 if OK, negative on error or end of file.
        /// </returns>
        public virtual int ReadFrame(AVFormatContext context, AVPacket packet)
        {
            return ffmpeg.av_read_frame(context.NativeObject, packet.NativeObject);
        }

        /// <summary>
        /// Wipe the packet.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        public virtual void UnrefPacket(AVPacket packet)
        {
            ffmpeg.av_packet_unref(packet.NativeObject);
        }

        /// <summary>
        /// Unreference all the buffers referenced by frame and reset the frame fields.
        /// </summary>
        /// <param name="frame">
        /// The frame.
        /// </param>
        public virtual void UnrefFrame(AVFrame frame)
        {
            ffmpeg.av_frame_unref(frame.NativeObject);
        }

        /// <summary>
        /// Close a given AVCodecContext and free all the data associated with it (but not he AVCodecContext itself).
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// THe native result.
        /// </returns>
        public virtual int CloseCodec(AVCodecContext* context)
        {
            return ffmpeg.avcodec_close(context);
        }

        /// <summary>
        /// Close an opened input AVFormatContext. Free it and all its contents and set *s to NULL.
        /// </summary>
        /// <param name="formatContext">
        /// The format context.
        /// </param>
        public virtual void CloseFormatContext(AVFormatContext formatContext)
        {
            var native = formatContext.NativeObject;
            ffmpeg.avformat_close_input(&native);
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if a FFmpeg return value indicates a failure.
        /// </summary>
        /// <param name="ret">
        /// The return value based on which to determine whether a <see cref="InvalidOperationException"/> should be thrown.
        /// </param>
        /// <param name="postiveIndicatesSuccess">
        /// <see langword="true"/> if positive numbers indicate success; <see langword="false"/> if only strictly zero values
        /// indicate success. Negative values always indicate a failure.
        /// </param>
        public virtual void ThrowOnAVError(int ret, bool postiveIndicatesSuccess = true)
        {
            if (ret == 0 || (postiveIndicatesSuccess && ret > 0))
            {
                return;
            }

            if (ret < 0 && ret >= -1024)
            {
                // Values -1 through -1024 are Unix-specific error code
                UnixMarshal.ThrowExceptionForError((UnixError)(-ret));
            }
            else
            {
                throw new InvalidOperationException("FFmpeg returned error {ret}");
            }
        }
    }
}
