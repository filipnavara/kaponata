// <copyright file="UdifBuffer.cs" company="Kenneth Bell, Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// Copyright (c) 2008-2011, Kenneth Bell
// </copyright>
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable disable

using DiscUtils.Compression;
using DiscUtils.Streams;
using Kaponata.FileFormats.Lzma;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Buffer=DiscUtils.Streams.Buffer;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// A <see cref="Buffer"/> which supports reading data from a DMG file.
    /// </summary>
    public class UdifBuffer : Buffer
    {
        private readonly ResourceFork resources;
        private readonly long sectorCount;
        private readonly Stream stream;

        private CompressedRun activeRun;
        private long activeRunOffset;

        private byte[] decompBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdifBuffer"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which provides access to the raw dmg image.
        /// </param>
        /// <param name="resources">
        /// The resource fork.
        /// </param>
        /// <param name="sectorCount">
        /// The total number of sectors in the disk image.
        /// </param>
        public UdifBuffer(Stream stream, ResourceFork resources, long sectorCount)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
            this.sectorCount = sectorCount;

            this.Blocks = new List<CompressedBlock>();

            foreach (Resource resource in this.resources.GetAllResources("blkx"))
            {
                this.Blocks.Add(((BlkxResource)resource).Block);
            }
        }

        /// <summary>
        /// Gets a list of blocks which provide access to the raw, compressed data.
        /// </summary>
        public List<CompressedBlock> Blocks { get; }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override long Capacity
        {
            get { return this.sectorCount * Sizes.Sector; }
        }

        /// <inheritdoc/>
        public override int Read(long pos, byte[] buffer, int offset, int count)
        {
            int totalCopied = 0;
            long currentPos = pos;

            while (totalCopied < count && currentPos < this.Capacity)
            {
                this.LoadRun(currentPos);

                int bufferOffset = (int)(currentPos - (this.activeRunOffset + (this.activeRun.SectorStart * Sizes.Sector)));
                int toCopy = (int)Math.Min((this.activeRun.SectorCount * Sizes.Sector) - bufferOffset, count - totalCopied);

                switch (this.activeRun.Type)
                {
                    case RunType.Zeros:
                    case RunType.None:
                        Array.Clear(buffer, offset + totalCopied, toCopy);
                        break;

                    case RunType.Raw:
                        this.stream.Position = this.activeRun.CompOffset + bufferOffset;
                        StreamUtilities.ReadExact(this.stream, buffer, offset + totalCopied, toCopy);
                        break;

                    case RunType.AdcCompressed:
                    case RunType.ZlibCompressed:
                    case RunType.BZlibCompressed:
                    case RunType.LzfseCompressed:
                    case RunType.LzmaCompressed:
                        Array.Copy(this.decompBuffer, bufferOffset, buffer, offset + totalCopied, toCopy);
                        break;

                    default:
                        throw new NotImplementedException("Reading from run of type " + this.activeRun.Type);
                }

                currentPos += toCopy;
                totalCopied += toCopy;
            }

            return totalCopied;
        }

        /// <inheritdoc/>
        public override void Write(long pos, byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetCapacity(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override IEnumerable<StreamExtent> GetExtentsInRange(long start, long count)
        {
            StreamExtent lastRun = null;

            foreach (CompressedBlock block in this.Blocks)
            {
                if ((block.FirstSector + block.SectorCount) * Sizes.Sector < start)
                {
                    // Skip blocks before start of range
                    continue;
                }

                if (block.FirstSector * Sizes.Sector > start + count)
                {
                    // Skip blocks after end of range
                    continue;
                }

                foreach (CompressedRun run in block.Runs)
                {
                    if (run.SectorCount > 0 && run.Type != RunType.Zeros)
                    {
                        long thisRunStart = (block.FirstSector + run.SectorStart) * Sizes.Sector;
                        long thisRunEnd = thisRunStart + (run.SectorCount * Sizes.Sector);

                        thisRunStart = Math.Max(thisRunStart, start);
                        thisRunEnd = Math.Min(thisRunEnd, start + count);

                        long thisRunLength = thisRunEnd - thisRunStart;

                        if (thisRunLength > 0)
                        {
                            if (lastRun != null && lastRun.Start + lastRun.Length == thisRunStart)
                            {
                                lastRun = new StreamExtent(lastRun.Start, lastRun.Length + thisRunLength);
                            }
                            else
                            {
                                if (lastRun != null)
                                {
                                    yield return lastRun;
                                }

                                lastRun = new StreamExtent(thisRunStart, thisRunLength);
                            }
                        }
                    }
                }
            }

            if (lastRun != null)
            {
                yield return lastRun;
            }
        }

        private static int ADCDecompress(
            byte[] inputBuffer,
            int inputOffset,
            int inputCount,
            byte[] outputBuffer,
            int outputOffset)
        {
            int consumed = 0;
            int written = 0;

            while (consumed < inputCount)
            {
                byte focusByte = inputBuffer[inputOffset + consumed];
                if ((focusByte & 0x80) != 0)
                {
                    // Data Run
                    int chunkSize = (focusByte & 0x7F) + 1;
                    Array.Copy(inputBuffer, inputOffset + consumed + 1, outputBuffer, outputOffset + written, chunkSize);

                    consumed += chunkSize + 1;
                    written += chunkSize;
                }
                else if ((focusByte & 0x40) != 0)
                {
                    // 3 byte code
                    int chunkSize = (focusByte & 0x3F) + 4;
                    int offset = EndianUtilities.ToUInt16BigEndian(inputBuffer, inputOffset + consumed + 1);

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        outputBuffer[outputOffset + written + i] =
                            outputBuffer[outputOffset + written + i - offset - 1];
                    }

                    consumed += 3;
                    written += chunkSize;
                }
                else
                {
                    // 2 byte code
                    int chunkSize = ((focusByte & 0x3F) >> 2) + 3;
                    int offset = ((focusByte & 0x3) << 8) + (inputBuffer[inputOffset + consumed + 1] & 0xFF);

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        outputBuffer[outputOffset + written + i] =
                            outputBuffer[outputOffset + written + i - offset - 1];
                    }

                    consumed += 2;
                    written += chunkSize;
                }
            }

            return written;
        }

        private void LoadRun(long pos)
        {
            if (this.activeRun != null
                && pos >= this.activeRunOffset + (this.activeRun.SectorStart * Sizes.Sector)
                && pos < this.activeRunOffset + ((this.activeRun.SectorStart + this.activeRun.SectorCount) * Sizes.Sector))
            {
                return;
            }

            long findSector = pos / 512;
            foreach (CompressedBlock block in this.Blocks)
            {
                if (block.FirstSector <= findSector && block.FirstSector + block.SectorCount > findSector)
                {
                    // Make sure the decompression buffer is big enough
                    if (this.decompBuffer == null || this.decompBuffer.Length < block.DecompressBufferRequested * Sizes.Sector)
                    {
                        this.decompBuffer = new byte[block.DecompressBufferRequested * Sizes.Sector];
                    }

                    foreach (CompressedRun run in block.Runs)
                    {
                        if (block.FirstSector + run.SectorStart <= findSector &&
                            block.FirstSector + run.SectorStart + run.SectorCount > findSector)
                        {
                            this.LoadRun(run);
                            this.activeRunOffset = block.FirstSector * Sizes.Sector;
                            return;
                        }
                    }

                    throw new IOException("No run for sector " + findSector + " in block starting at " +
                                          block.FirstSector);
                }
            }

            throw new IOException("No block for sector " + findSector);
        }

        private void LoadRun(CompressedRun run)
        {
            int toCopy = (int)(run.SectorCount * Sizes.Sector);

            switch (run.Type)
            {
                case RunType.ZlibCompressed:
                    this.stream.Position = run.CompOffset + 2; // 2 byte zlib header
                    using (DeflateStream ds = new DeflateStream(this.stream, CompressionMode.Decompress, true))
                    {
                        StreamUtilities.ReadExact(ds, this.decompBuffer, 0, toCopy);
                    }

                    break;

                case RunType.AdcCompressed:
                    this.stream.Position = run.CompOffset;
                    byte[] compressed = StreamUtilities.ReadExact(this.stream, (int)run.CompLength);
                    if (ADCDecompress(compressed, 0, compressed.Length, this.decompBuffer, 0) != toCopy)
                    {
                        throw new InvalidDataException("Run too short when decompressed");
                    }

                    break;

                case RunType.BZlibCompressed:
                    using (
                        BZip2DecoderStream ds =
                            new BZip2DecoderStream(
                                new SubStream(
                                    this.stream,
                                    run.CompOffset,
                                    run.CompLength),
                                Ownership.None))
                    {
                        StreamUtilities.ReadExact(ds, this.decompBuffer, 0, toCopy);
                    }

                    break;

                case RunType.LzfseCompressed:
                    this.stream.Position = run.CompOffset;
                    byte[] lzfseCompressed = StreamUtilities.ReadExact(this.stream, (int)run.CompLength);
                    if (Lzfse.LzfseCompressor.Decompress(lzfseCompressed, this.decompBuffer) != toCopy)
                    {
                        throw new InvalidDataException("Run too short when decompressed");
                    }

                    break;

                case RunType.LzmaCompressed:
                    using (var ds = new XZInputStream(
                                new SubStream(
                                    this.stream,
                                    run.CompOffset,
                                    run.CompLength)))
                    {
                        StreamUtilities.ReadExact(ds, this.decompBuffer, 0, toCopy);
                    }

                    break;

                case RunType.Zeros:
                case RunType.Raw:
                case RunType.None:
                    break;

                default:
                    throw new NotImplementedException("Unrecognized run type " + run.Type);
            }

            this.activeRun = run;
        }
    }
}