// <copyright file="PbzxStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using DiscUtils.Streams;
using Kaponata.FileFormats.Lzma;
using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;

namespace Kaponata.FileFormats.Pbzx
{
    /// <summary>
    /// Reads data embedded within a pbzx file. The pbzx file format embeds an xz file in chunks.
    /// </summary>
    /// <seealso href="http://newosxbook.com/articles/OTA.html"/>
    /// <seealso href="http://newosxbook.com/src.jl?tree=listings&amp;file=pbzx.c"/>
    public class PbzxStream : Stream
    {
        private readonly Ownership ownership;

        /// <summary>
        /// The magic for the PBZX file.
        /// </summary>
        private const int Magic = 0x787a6270;

        /// <summary>
        /// The header of a xz chunk.
        /// </summary>
        private const int ZxHeader = 0x587a37fd;

        /// <summary>
        /// The footer of a xz chunk.
        /// </summary>
        private const short ZxFooter = 0x5a59;

        private readonly Stream stream;

        /// <summary>
        /// The _decompressed_ size of an individual chunk.
        /// </summary>
        private readonly long chunksize;
        private long leftInChunk;
        private bool isLastChunk;

        // pbzx files are split up in different blocks. Each block decompresses to the
        // same amount of data. This is typically 0x100_0000 bytes (~ 16 MB).
        private readonly IMemoryOwner<byte> decompressedDataBuffer;
        private Memory<byte> decompressedData;
        private readonly byte[] header = new byte[16];

        private long position = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PbzxStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The underlying stream around which this <see cref="PbzxStream"/> wraps.
        /// </param>
        public PbzxStream(Stream stream, Ownership ownership)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.ownership = ownership;

            this.stream = stream;

            byte[] header = new byte[12];
            this.stream.Read(header, 0, 12);
            var magic = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(0, 4));

            if (magic != Magic)
            {
                throw new InvalidDataException("The file is not a pbzx file");
            }

            this.chunksize = BinaryPrimitives.ReadInt64BigEndian(header.AsSpan(4, 8));
            this.decompressedDataBuffer = MemoryPool<byte>.Shared.Rent((int)this.chunksize);
        }

        /// <inhheritdoc/>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <inhheritdoc/>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <inhheritdoc/>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inhheritdoc/>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <inhheritdoc/>
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        private XZDecompressor decompressor = new XZDecompressor(LzmaFormat.Xz);
        private Stream? chunkStream = null;

        private int bytesLeftInChunk = 0;

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return 0;
            }

            if (this.bytesLeftInChunk == 0)
            {
                if (this.isLastChunk)
                {
                    // That's all there is
                    return 0;
                }

                this.ReadChunk();
            }

            if (this.chunkStream == null)
            {
                int canRead = Math.Min(this.decompressedData.Length, count);
                this.decompressedData.Slice(0, canRead).CopyTo(new Memory<byte>(buffer, offset, canRead));
                this.decompressedData = this.decompressedData.Slice(canRead);
                this.bytesLeftInChunk -= canRead;
                this.position += canRead;
                return canRead;
            }
            else
            {
                int didRead = this.chunkStream.Read(buffer, offset, count);
                this.bytesLeftInChunk -= didRead;
                this.position += didRead;
                return didRead;
            }
        }

        private void ReadChunk()
        {
            this.stream.Read(this.header, 0, 16);

            var flags = BinaryPrimitives.ReadUInt64BigEndian(this.header.AsSpan(0, 8));
            this.isLastChunk = (flags & 0x01000000) == 0;
            this.leftInChunk = BinaryPrimitives.ReadInt64BigEndian(this.header.AsSpan(8, 8));
            Debug.Assert(this.leftInChunk <= this.chunksize);

            Debug.WriteLine($"Entering a new chunk of size 0x{this.leftInChunk:X}. Flags: 0x{flags:X}");

            if (this.leftInChunk == this.chunksize)
            {
                this.chunkStream = this.stream.ReadSlice(this.leftInChunk);
            }
            else
            {
                using (var compressedDataOwner = MemoryPool<byte>.Shared.Rent((int)this.leftInChunk))
                {
                    var compressedData = compressedDataOwner.Memory.Slice(0, (int)this.leftInChunk);
                    this.decompressedData = this.decompressedDataBuffer.Memory.Slice(0, (int)this.chunksize);

                    this.stream.Read(compressedData.Span);
                    var result = this.decompressor.Decompress(
                        compressedData.Span,
                        this.decompressedData.Span,
                        out int bytesConsumed,
                        out int bytesWritten);

                    Debug.Assert(result == OperationStatus.NeedMoreData);
                    Debug.Assert(bytesConsumed == this.leftInChunk);
                    Debug.Assert(bytesWritten == this.chunksize);
                }

                this.chunkStream = null;
            }

            this.bytesLeftInChunk = (int)this.chunksize;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.ownership == Ownership.Dispose)
            {
                this.stream.Dispose();
            }

            this.decompressedDataBuffer.Dispose();
        }
    }
}
