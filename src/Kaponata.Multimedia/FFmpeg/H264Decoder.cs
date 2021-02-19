// <copyright file="H264Decoder.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NativeAVPictureType = FFmpeg.AutoGen.AVPictureType;
using NativeReadPacket = FFmpeg.AutoGen.avio_alloc_context_read_packet;
using NativeReadPacketFunc = FFmpeg.AutoGen.avio_alloc_context_read_packet_func;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Decodes a h264 stream.
    /// </summary>
    public class H264Decoder : IDisposable
    {
        /// <summary>
        /// The amount of bytes read at once (for data transmitted between our stream buffer and FFmpeg).
        /// </summary>
        protected const int ReadBufferSize = 0x10000; // 64 KB

        private readonly Stream? stream;
        private readonly FFmpegClient client;
        private readonly ILogger<H264Decoder> logger;

        /// <summary>
        /// The background task which decodes the current stream.
        /// </summary>
        private Task decodeTask = Task.CompletedTask;

        /// <summary>
        /// Used to cancel the <see cref="decodeTask"/>.
        /// </summary>
        private CancellationTokenSource? decodeCts;

        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="H264Decoder"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> from which to consume data.
        /// </param>
        public H264Decoder(Stream stream)
            : this(stream, NullLogger<H264Decoder>.Instance, new FFmpegClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="H264Decoder"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> from which to consume data.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="client">
        /// The ffmpeg client to access the FFmpeg API.
        /// </param>
        public H264Decoder(Stream stream, ILogger<H264Decoder> logger, FFmpegClient client)
        {
            this.client = client;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Gets an <see cref="Task"/> which represents the decode operation.
        /// </summary>
        public Task DecodeTask => this.decodeTask;

        /// <summary>
        /// Gets the framebuffer.
        /// </summary>
        public FrameBuffer FrameBuffer { get;  } = new FrameBuffer();

        /// <summary>
        /// Starts decoding the current stream as a background task.
        /// </summary>
        public virtual void Start()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(H264Decoder));
            }

            if (!this.decodeTask.IsCompleted)
            {
                throw new InvalidOperationException();
            }

            this.decodeCts = new CancellationTokenSource();
            this.decodeTask = Task.Run(() => this.Decode(this.decodeCts.Token));
        }

        /// <summary>
        /// Decodes the current stream and blocks until decoding is done.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        public virtual unsafe void Decode(CancellationToken cancellationToken = default)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(H264Decoder));
            }

            var readDelegate = new NativeReadPacket(this.Read);
            var readFunc = new NativeReadPacketFunc()
            {
                Pointer = Marshal.GetFunctionPointerForDelegate(readDelegate),
            };

            using (var ioContext = new AVIOContext(this.client, ReadBufferSize, readFunc, null))
            using (var formatContext = new AVFormatContext(this.client, ioContext))
            {
                formatContext.OpenInputStream("h264");
                var stream = formatContext.GetVideoStream();
                using (var codec = new AVCodec(this.client, stream))
                using (var frame = new AVFrame(this.client))
                using (var packet = new AVPacket(this.client))
                {
                    int frameNumber = 0;

                    while (packet.ReadFrame(formatContext))
                    {
                        this.logger.LogDebug($"Got a frame for stream {packet.StreamIndex}");

                        if (packet.NativeObject->stream_index != stream.Index)
                        {
                            continue;
                        }

                        this.logger.LogDebug("Sending packet");
                        codec.SendPacket(packet);

                        int framesInPacket = 0;

                        while (codec.ReceiveFrame(frame))
                        {
                            this.logger.LogDebug("Receiving frame");

                            framesInPacket += 1;
                            if (frame.PictureType != NativeAVPictureType.AV_PICTURE_TYPE_NONE)
                            {
                                this.logger.LogDebug($"Got a picture of {frame.Width}x{frame.Height} in color space {frame.Format}");

                                // decode frame
                                this.FrameBuffer.DecompresFrame(
                                    frame.Width,
                                    frame.Height,
                                    frame.Width,
                                    frame.Height,
                                    frame.Width * 4,
                                    new Span<byte>(frame.NativeObject->data[0], frame.NativeObject->linesize[0]),
                                    new Span<byte>(frame.NativeObject->data[1], frame.NativeObject->linesize[1]),
                                    new Span<byte>(frame.NativeObject->data[2], frame.NativeObject->linesize[2]),
                                    new int[] { frame.NativeObject->linesize[0], frame.NativeObject->linesize[1], frame.NativeObject->linesize[2] });
                            }
                        }

                        this.logger.LogInformation($"Add {framesInPacket} frames in packet.");
                        frameNumber++;
                    }
                }
            }

            GC.KeepAlive(readDelegate);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (!this.decodeTask.IsCompleted)
            {
                this.decodeCts?.Cancel();
            }

            this.FrameBuffer.Dispose();
            this.disposed = true;
        }

        /// <summary>
        /// Reads data from the stream buffer into FFmpeg.
        /// </summary>
        /// <param name="opaque">
        /// An opaque handle provided by us. Ignored.
        /// </param>
        /// <param name="buffer">
        /// The buffer into which to read the data.
        /// </param>
        /// <param name="size">
        /// The maximum amount of bytes to read.
        /// </param>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        private unsafe int Read(void* opaque, byte* buffer, int size)
        {
            // This is a native callback. Letting an exception propagate causes the entire process to crash.
            try
            {
                var read = this.stream?.Read(new Span<byte>(buffer, size));
                return read.GetValueOrDefault();
            }
            catch (Exception ex)
            {
                this.logger.LogInformation($"An unexpected error occurred while reading from the stream: {ex.Message}. Assuming the stream is closed.");
                return 0;
            }
        }
    }
}
