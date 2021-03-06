// <copyright file="XZOutputStream.cs" company="Quamotion bv">
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

using Microsoft;
using System;
using System.Diagnostics;
using System.IO;

#nullable disable

namespace Kaponata.FileFormats.Lzma
{
    /// <summary>
    /// A <see cref="Stream"/> which writes XZ (or LZMA)-compressed data.
    /// </summary>
    public unsafe class XZOutputStream : Stream, IDisposableObservable
    {
        /// <summary>
        /// Default compression preset.
        /// </summary>
        public const uint DefaultPreset = 6;

        /// <summary>
        /// The extreme compression preset.
        /// </summary>
        public const uint PresetExtremeFlag = 1U << 31;

        // You can tweak BufSize value to get optimal results
        // of speed and chunk size
        private const int BufSize = 4096;

        private readonly Stream innerStream;
        private readonly bool leaveOpen;
        private readonly byte[] outbuf;
        private LzmaStream lzmaStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="XZOutputStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write compressed data.
        /// </param>
        public XZOutputStream(Stream stream)
            : this(stream, DefaultThreads)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XZOutputStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write compressed data.
        /// </param>
        /// <param name="threads">
        /// The number of threads to use when compressing data.
        /// </param>
        public XZOutputStream(Stream stream, int threads)
            : this(stream, threads, DefaultPreset)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XZOutputStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write compressed data.
        /// </param>
        /// <param name="threads">
        /// The number of threads to use when compressing data.
        /// </param>
        /// <param name="preset">
        /// The compression preset to use.
        /// </param>
        public XZOutputStream(Stream stream, int threads, uint preset)
            : this(stream, threads, preset, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XZOutputStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write compressed data.
        /// </param>
        /// <param name="threads">
        /// The number of threads to use when compressing data.
        /// </param>
        /// <param name="preset">
        /// The compression preset to use.
        /// </param>
        /// <param name="leaveOpen">
        /// A value indicating whether to dispose of <paramref name="stream"/> when this <see cref="XZOutputStream"/>
        /// is disposed of, or not.
        /// </param>
        public XZOutputStream(Stream stream, int threads, uint preset, bool leaveOpen)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.innerStream = stream;
            this.leaveOpen = leaveOpen;

            LzmaResult ret;
            if (threads == 1 || !NativeMethods.SupportsMultiThreading)
            {
                ret = NativeMethods.lzma_easy_encoder(ref this.lzmaStream, preset, LzmaCheck.Crc64);
            }
            else
            {
                if (threads <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(threads));
                }

                if (threads > Environment.ProcessorCount)
                {
                    Trace.TraceWarning("{0} threads required, but only {1} processors available", threads, Environment.ProcessorCount);
                    threads = Environment.ProcessorCount;
                }

                var mt = new LzmaMT()
                {
                    preset = preset,
                    check = LzmaCheck.Crc64,
                    threads = (uint)threads,
                };
                ret = NativeMethods.lzma_stream_encoder_mt(ref this.lzmaStream, ref mt);
            }

            if (ret == LzmaResult.OK)
            {
                this.outbuf = new byte[BufSize];
                this.lzmaStream.AvailOut = BufSize;
                return;
            }

            GC.SuppressFinalize(this);
            LzmaException.ThrowOnError(ret);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="XZOutputStream"/> class.
        /// </summary>
        ~XZOutputStream()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the default numbers of threads to use.
        /// </summary>
        public static int DefaultThreads => Environment.ProcessorCount;

        /// <summary>
        /// Gets a value indicating whether multithreading is supported.
        /// </summary>
        public static bool SupportsMultiThreading => NativeMethods.SupportsMultiThreading;

        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                Verify.NotDisposed(this);
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                Verify.NotDisposed(this);
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                Verify.NotDisposed(this);
                return true;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                Verify.NotDisposed(this);
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                Verify.NotDisposed(this);
                throw new NotSupportedException();
            }

            set
            {
                Verify.NotDisposed(this);
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Single-call buffer encoding.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to encode.
        /// </param>
        /// <param name="preset">
        /// The compression preset to use.
        /// </param>
        /// <returns>
        /// A <see cref="byte"/> array which contains the compressed data.
        /// </returns>
        public static byte[] Encode(byte[] buffer, uint preset = DefaultPreset)
        {
            var res = new byte[(long)NativeMethods.lzma_stream_buffer_bound((UIntPtr)buffer.Length)];

            UIntPtr outPos;
            var ret = NativeMethods.lzma_easy_buffer_encode(preset, LzmaCheck.Crc64, null, buffer, (UIntPtr)buffer.Length, res, &outPos, (UIntPtr)res.Length);
            LzmaException.ThrowOnError(ret);

            if ((long)outPos < res.Length)
            {
                Array.Resize(ref res, (int)(ulong)outPos);
            }

            return res;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            Verify.NotDisposed(this);
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            Verify.NotDisposed(this);
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            Verify.NotDisposed(this);
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Verify.NotDisposed(this);
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Verify.NotDisposed(this);

            if (count == 0)
            {
                return;
            }

            var guard = buffer[checked((uint)offset + (uint)count) - 1];

            if (this.lzmaStream.AvailIn != 0)
            {
                throw new InvalidOperationException();
            }

            this.lzmaStream.AvailIn = (uint)count;
            do
            {
                LzmaResult ret;
                fixed (byte* inbuf = &buffer[offset])
                {
                    this.lzmaStream.NextIn = inbuf;
                    fixed (byte* outbuf = &this.outbuf[BufSize - this.lzmaStream.AvailOut])
                    {
                        this.lzmaStream.NextOut = outbuf;
                        ret = NativeMethods.lzma_code(ref this.lzmaStream, LzmaAction.Run);
                    }

                    offset += (int)((ulong)this.lzmaStream.NextIn - (ulong)(IntPtr)inbuf);
                }

                if (ret != LzmaResult.OK)
                {
                    NativeMethods.lzma_end(ref this.lzmaStream);
                    LzmaException.ThrowOnError(ret);
                }

                if (this.lzmaStream.AvailOut == 0)
                {
                    this.innerStream.Write(this.outbuf, 0, BufSize);
                    this.lzmaStream.AvailOut = BufSize;
                }
            }
            while (this.lzmaStream.AvailIn != 0);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // finish encoding only if all input has been successfully processed
            if (this.lzmaStream.InternalState != null && this.lzmaStream.AvailIn == 0)
            {
                LzmaResult ret;
                do
                {
                    fixed (byte* outbuf = &this.outbuf[BufSize - (int)this.lzmaStream.AvailOut])
                    {
                        this.lzmaStream.NextOut = outbuf;
                        ret = NativeMethods.lzma_code(ref this.lzmaStream, LzmaAction.Finish);
                    }

                    if (ret > LzmaResult.StreamEnd)
                    {
                        NativeMethods.lzma_end(ref this.lzmaStream);
                        LzmaException.ThrowOnError(ret);
                    }

                    var writeSize = BufSize - (int)this.lzmaStream.AvailOut;
                    if (writeSize != 0)
                    {
                        this.innerStream.Write(this.outbuf, 0, writeSize);
                        this.lzmaStream.AvailOut = BufSize;
                    }
                }
                while (ret != LzmaResult.StreamEnd);
            }

            NativeMethods.lzma_end(ref this.lzmaStream);

            if (disposing && !this.leaveOpen)
            {
                this.innerStream?.Dispose();
            }

            base.Dispose(disposing);

            this.IsDisposed = true;
        }
    }
}