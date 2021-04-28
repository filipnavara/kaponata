// <copyright file="XZDecompressor.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Buffers;

namespace Kaponata.FileFormats.Lzma
{
    /// <summary>
    /// Provides XZ and lzma decompression methods. The methods decompress in a single pass without using a <see cref="XZInputStream"/> instance.
    /// </summary>
    public class XZDecompressor : IDisposable
    {
        private LzmaStream lzmaStream;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="XZDecompressor" /> class.
        /// </summary>
        /// <param name="format">
        /// The format of the data to decompress.
        /// </param>
        public XZDecompressor(LzmaFormat format = LzmaFormat.Auto)
        {
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

            LzmaException.ThrowOnError(ret);
        }

        /// <summary>
        /// Decompresses data that was compressed using the xz or lzma algorithm.
        /// </summary>
        /// <param name="source">
        /// A buffer containing the compressed data.
        /// </param>
        /// <param name="destination">
        /// When this method returns, a byte span containing the decompressed data.
        /// </param>
        /// <param name="bytesConsumed">
        /// The total number of bytes that were read from <paramref name="source"/>.
        /// </param>
        /// <param name="bytesWritten">
        /// The total number of bytes that were written in the <paramref name="destination"/>.
        /// </param>
        /// <returns>
        /// One of the enumeration values that indicates the status of the decompression operation.
        /// </returns>
        public unsafe OperationStatus Decompress(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten)
        {
            this.EnsureNotDisposed();

            // Make sure data is available in the output buffer.
            fixed (byte* sourcePtr = source)
            fixed (byte* destinationPtr = destination)
            {
                this.lzmaStream.NextIn = sourcePtr;
                this.lzmaStream.AvailIn = (uint)source.Length;

                this.lzmaStream.NextOut = destinationPtr;
                this.lzmaStream.AvailOut = (uint)destination.Length;

                var ret = NativeMethods.lzma_code(ref this.lzmaStream, source.Length == 0 ? LzmaAction.Finish : LzmaAction.Run);

                bytesConsumed = source.Length - (int)this.lzmaStream.AvailIn;
                bytesWritten = destination.Length - (int)this.lzmaStream.AvailOut;

                switch (ret)
                {
                    case LzmaResult.OK:
                        return OperationStatus.NeedMoreData;

                    case LzmaResult.StreamEnd:
                        return OperationStatus.Done;

                    case LzmaResult.FormatError:
                    case LzmaResult.DataError:
                        NativeMethods.lzma_end(ref this.lzmaStream);
                        return OperationStatus.InvalidData;

                    default:
                        NativeMethods.lzma_end(ref this.lzmaStream);
                        throw new LzmaException(ret);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            NativeMethods.lzma_end(ref this.lzmaStream);

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
