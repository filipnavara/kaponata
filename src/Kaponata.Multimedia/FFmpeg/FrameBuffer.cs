// <copyright file="FrameBuffer.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.TurboJpeg;
using System;
using System.Buffers;
using System.Threading;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    ///  Stores the raw picture data in DestinationPixelFormat.
    /// </summary>
    public class FrameBuffer : IDisposable
    {
        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        private readonly TJDecompressor decompressor = new TJDecompressor();

        private ReaderWriterLockSlim framebufferLock = new ReaderWriterLockSlim();
        private IMemoryOwner<byte>? buffer;
        private bool disposed = false;

        /// <summary>
        /// An event which is updated when a new frame has been received and put in the framebuffer.
        /// </summary>
        public virtual event EventHandler? FrameReceived;

        /// <summary>
        /// Gets or sets the destination pixel format.
        /// </summary>
        public TJPixelFormat DestinationPixelFormat { get; set; } = TJPixelFormat.BGRA;

        /// <summary>
        /// Gets the width of the current frame.
        /// </summary>
        public virtual int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the current frame.
        /// </summary>
        public virtual int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the aligned width of the current frame.
        /// </summary>
        public int AlignedWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the aligned height of the current frame.
        /// </summary>
        public int AlignedHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the stride of the current frame. The stride is the number of bytes from one row of pixels in memory to the next row of pixels in memory.
        /// In the general case, the stide is equal to the number of pixels in a row multiplied by the bytes per pixel. However, If padding bytes are present,
        /// the stride is wider than the width of the image.
        /// </summary>
        public int Stride
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size, in bytes, of the current framebuffer.
        /// </summary>
        public int FrameBufferSize => this.Stride * this.Height;

        /// <summary>
        /// Decompresses the received frame.
        /// </summary>
        /// <param name="width">
        /// The width of the frame.
        /// </param>
        /// <param name="height">
        /// The height of the frame.
        /// </param>
        /// <param name="alignedWidth">
        /// The aligned width of the frame.
        /// </param>
        /// <param name="alignedHeight">
        /// The aligned height of the frame.
        /// </param>
        /// <param name="stride">
        /// The stride of the frame.
        /// </param>
        /// <param name="yPlane">
        /// The y plane.
        /// </param>
        /// <param name="uPlane">
        /// The u plane.
        /// </param>
        /// <param name="vPlane">
        /// The v plane.
        /// </param>
        /// <param name="strides">
        /// the strides.
        /// </param>
        public unsafe virtual void DecompresFrame(int width, int height, int alignedWidth, int alignedHeight, int stride, Span<byte> yPlane, Span<byte> uPlane, Span<byte> vPlane, int[] strides)
        {
            this.framebufferLock.EnterWriteLock();

            try
            {
                this.Width = width;
                this.Height = height;
                this.AlignedHeight = alignedHeight;
                this.AlignedWidth = alignedWidth;
                this.Stride = stride;

                if (this.buffer == null || this.buffer.Memory.Length < this.FrameBufferSize)
                {
                    this.buffer?.Dispose();
                    this.buffer = this.memoryPool.Rent(this.FrameBufferSize);
                }

                this.decompressor.DecodeYUVPlanes(
                    yPlane,
                    uPlane,
                    vPlane,
                    strides,
                    TJSubsamplingOption.Chrominance420,
                    this.buffer.Memory.Slice(0, this.FrameBufferSize).Span,
                    this.Width,
                    this.Stride,
                    this.Height,
                    this.DestinationPixelFormat,
                    TJFlags.NoRealloc);
            }
            finally
            {
                this.framebufferLock.ExitWriteLock();
                this.OnFrameReceived();
            }
        }

        /// <summary>
        /// Copies the current framebuffer to a byte array.
        /// </summary>
        /// <param name="buffer">
        /// the destination buffer.
        /// </param>
        public virtual void CopyFramebuffer(Memory<byte> buffer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(H264Decoder));
            }

            try
            {
                this.framebufferLock.EnterReadLock();

                if (this.buffer == null)
                {
                    throw new InvalidOperationException("The buffer is not initialized.");
                }

                if (buffer.Length < this.FrameBufferSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(buffer));
                }

                this.buffer.Memory.CopyTo(buffer);
            }
            finally
            {
                this.framebufferLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Invokes the <see cref="FrameBuffer.FrameReceived"/> event. Use only for test purposes.
        /// </summary>
        public void OnFrameReceived()
        {
            this.FrameReceived?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.buffer?.Dispose();
            this.framebufferLock.Dispose();
            this.decompressor.Dispose();
            this.disposed = true;
        }
    }
}
