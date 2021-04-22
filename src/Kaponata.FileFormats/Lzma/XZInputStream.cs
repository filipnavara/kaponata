// <copyright file="XZInputStream.cs" company="Quamotion bv">
// Copyright(c) 2015 Roman Belkov, Kirill Melentyev
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Kaponata.FileFormats.Lzma
{
    /// <summary>
    /// Represents a <see cref="Stream"/> which can decompress xz-compressed data.
    /// </summary>
    public class XZInputStream : Stream
    {
        /// <summary>
        /// The size of the buffer.
        /// </summary>
        private const int BufSize = 512;

        private readonly List<byte> internalBuffer = new List<byte>();
        private readonly Stream innerStream;
        private readonly IntPtr inbuf;
        private readonly IntPtr outbuf;
        private LzmaStream lzmaStream;
        private long length;
        private long position;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="XZInputStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The underlying <see cref="Stream"/> from which to decompress the data.
        /// </param>
        /// <param name="format">
        /// The lzma formats which are supported.
        /// </param>
        public XZInputStream(Stream stream, LzmaFormat format = LzmaFormat.Auto)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.innerStream = stream;

            LzmaResult ret;

            switch (format)
            {
                case LzmaFormat.Lzma:
                    ret = NativeMethods.lzma_alone_decoder(ref this.lzmaStream, ulong.MaxValue);
                    break;

                case LzmaFormat.Xz:
                    ret = NativeMethods.lzma_stream_decoder(ref this.lzmaStream, ulong.MaxValue, LzmaDecodeFlags.Concatenated);
                    break;

                default:
                case LzmaFormat.Auto:
                    ret = NativeMethods.lzma_auto_decoder(ref this.lzmaStream, ulong.MaxValue, LzmaDecodeFlags.Concatenated);
                    break;
            }

            this.inbuf = Marshal.AllocHGlobal(BufSize);
            this.outbuf = Marshal.AllocHGlobal(BufSize);

            this.lzmaStream.AvailIn = 0;
            this.lzmaStream.NextOut = this.outbuf;
            this.lzmaStream.AvailOut = BufSize;

            LzmaException.ThrowOnError(ret);
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return true;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                this.EnsureNotDisposed();
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return false;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                this.EnsureNotDisposed();

                const int streamFooterSize = 12;

                if (this.length == 0)
                {
                    var lzmaStreamFlags = default(LzmaStreamFlags);
                    var streamFooter = new byte[streamFooterSize];

                    this.innerStream.Seek(-streamFooterSize, SeekOrigin.End);
                    this.innerStream.Read(streamFooter, 0, streamFooterSize);

                    NativeMethods.lzma_stream_footer_decode(ref lzmaStreamFlags, streamFooter);
                    var indexPointer = new byte[lzmaStreamFlags.BackwardSize];

                    this.innerStream.Seek(-streamFooterSize - (long)lzmaStreamFlags.BackwardSize, SeekOrigin.End);
                    this.innerStream.Read(indexPointer, 0, (int)lzmaStreamFlags.BackwardSize);
                    this.innerStream.Seek(0, SeekOrigin.Begin);

                    var index = IntPtr.Zero;
                    var memLimit = ulong.MaxValue;
                    uint inPos = 0;

                    NativeMethods.lzma_index_buffer_decode(ref index, ref memLimit, IntPtr.Zero, indexPointer, ref inPos, lzmaStreamFlags.BackwardSize);

                    if (inPos != lzmaStreamFlags.BackwardSize)
                    {
                        NativeMethods.lzma_index_end(index, IntPtr.Zero);
                        throw new LzmaException("Index decoding failed!");
                    }

                    var uSize = NativeMethods.lzma_index_uncompressed_size(index);

                    NativeMethods.lzma_index_end(index, IntPtr.Zero);
                    this.length = (long)uSize;
                    return this.length;
                }
                else
                {
                    return this.length;
                }
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                this.EnsureNotDisposed();
                return this.position;
            }

            set
            {
                this.EnsureNotDisposed();
                throw new NotSupportedException("XZ Stream does not support setting position");
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            this.EnsureNotDisposed();

            throw new NotSupportedException("XZ Stream does not support flush");
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            this.EnsureNotDisposed();

            throw new NotSupportedException("XZ Stream does not support seek");
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            this.EnsureNotDisposed();

            throw new NotSupportedException("XZ Stream does not support setting length");
        }

        /// <summary>
        /// Reads bytes from stream.
        /// </summary>
        /// <param name="buffer">
        /// The buffer into which to read the data.
        /// </param>
        /// <param name="offset">
        /// The offset at which to start writing the data.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read.
        /// </param>
        /// <returns>byte read or -1 on end of stream.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            this.EnsureNotDisposed();

            var action = LzmaAction.Run;

            var readBuf = new byte[BufSize];
            var outManagedBuf = new byte[BufSize];

            while (this.internalBuffer.Count < count)
            {
                if (this.lzmaStream.AvailIn == 0)
                {
                    this.lzmaStream.AvailIn = (uint)this.innerStream.Read(readBuf, 0, readBuf.Length);
                    Marshal.Copy(readBuf, 0, this.inbuf, (int)this.lzmaStream.AvailIn);
                    this.lzmaStream.NextIn = this.inbuf;

                    if (this.lzmaStream.AvailIn == 0)
                    {
                        action = LzmaAction.Finish;
                    }
                }

                var ret = NativeMethods.lzma_code(ref this.lzmaStream, action);

                if (this.lzmaStream.AvailOut == 0 || ret == LzmaResult.StreamEnd)
                {
                    var writeSize = BufSize - (int)this.lzmaStream.AvailOut;
                    Marshal.Copy(this.outbuf, outManagedBuf, 0, writeSize);

                    this.internalBuffer.AddRange(outManagedBuf);
                    var tail = outManagedBuf.Length - writeSize;
                    this.internalBuffer.RemoveRange(this.internalBuffer.Count - tail, tail);

                    this.lzmaStream.NextOut = this.outbuf;
                    this.lzmaStream.AvailOut = BufSize;
                }

                if (ret != LzmaResult.OK)
                {
                    if (ret == LzmaResult.StreamEnd)
                    {
                        break;
                    }

                    NativeMethods.lzma_end(ref this.lzmaStream);

                    LzmaException.ThrowOnError(ret);
                }
            }

            if (this.internalBuffer.Count >= count)
            {
                this.internalBuffer.CopyTo(0, buffer, offset, count);
                this.internalBuffer.RemoveRange(0, count);
                this.position += count;
                return count;
            }
            else
            {
                var intBufLength = this.internalBuffer.Count;
                this.internalBuffer.CopyTo(0, buffer, offset, intBufLength);
                this.internalBuffer.Clear();
                this.position += intBufLength;
                return intBufLength;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureNotDisposed();

            throw new NotSupportedException("XZ Input stream does not support writing");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            NativeMethods.lzma_end(ref this.lzmaStream);

            Marshal.FreeHGlobal(this.inbuf);
            Marshal.FreeHGlobal(this.outbuf);

            base.Dispose(disposing);

            this.disposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(XZInputStream));
            }
        }
    }
}