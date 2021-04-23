// <copyright file="FileBuffer.cs" company="Kenneth Bell, Quamotion bv">
// Copyright (c) 2008-2011, Kenneth Bell
// Copyright (c) Quamotion bv. All rights reserved.
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

using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using Buffer=DiscUtils.Streams.Buffer;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A <see cref="Buffer"/> which provides access to files stored in a HFS+ file system.
    /// </summary>
    internal sealed class FileBuffer : Buffer
    {
        private readonly ForkData baseData;
        private readonly CatalogNodeId cnid;
        private readonly Context context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBuffer"/> class.
        /// </summary>
        /// <param name="context">
        /// The global HFS+ context.
        /// </param>
        /// <param name="baseData">
        /// The <see cref="ForkData"/> structure which represents the base file data.
        /// </param>
        /// <param name="catalogNodeId">
        /// The <see cref="CatalogNodeId"/> which uniquely identifies the file.
        /// </param>
        public FileBuffer(Context context, ForkData baseData, CatalogNodeId catalogNodeId)
        {
            this.context = context;
            this.baseData = baseData;
            this.cnid = catalogNodeId;
        }

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
            get { return (long)this.baseData.LogicalSize; }
        }

        /// <inheritdoc/>
        public override int Read(long pos, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;

            int limitedCount = (int)Math.Min(count, Math.Max(0, this.Capacity - pos));

            while (totalRead < limitedCount)
            {
                long extentLogicalStart;
                ExtentDescriptor extent = this.FindExtent(pos, out extentLogicalStart);
                long extentStreamStart = extent.StartBlock * (long)this.context.VolumeHeader.BlockSize;
                long extentSize = extent.BlockCount * (long)this.context.VolumeHeader.BlockSize;

                long extentOffset = pos + totalRead - extentLogicalStart;
                int toRead = (int)Math.Min(limitedCount - totalRead, extentSize - extentOffset);

                // Remaining in extent can create a situation where amount to read is zero, and that appears
                // to be OK, just need to exit the while loop to avoid infinite loop.
                if (toRead == 0)
                {
                    break;
                }

                Stream volStream = this.context.VolumeStream;
                volStream.Position = extentStreamStart + extentOffset;
                int numRead = volStream.Read(buffer, offset + totalRead, toRead);

                totalRead += numRead;
            }

            return totalRead;
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
            return new[] { new StreamExtent(start, Math.Min(start + count, this.Capacity) - start) };
        }

        private ExtentDescriptor FindExtent(long pos, out long extentLogicalStart)
        {
            uint blocksSeen = 0;
            uint block = (uint)(pos / this.context.VolumeHeader.BlockSize);
            for (int i = 0; i < this.baseData.Extents.Length; ++i)
            {
                if (blocksSeen + this.baseData.Extents[i].BlockCount > block)
                {
                    extentLogicalStart = blocksSeen * (long)this.context.VolumeHeader.BlockSize;
                    return this.baseData.Extents[i];
                }

                blocksSeen += this.baseData.Extents[i].BlockCount;
            }

            while (blocksSeen < this.baseData.TotalBlocks)
            {
                byte[] extentData = this.context.ExtentsOverflow.Find(new ExtentKey(this.cnid, blocksSeen, false));

                if (extentData != null)
                {
                    int extentDescriptorCount = extentData.Length / 8;
                    for (int a = 0; a < extentDescriptorCount; a++)
                    {
                        ExtentDescriptor extentDescriptor = new ExtentDescriptor();
                        int bytesRead = extentDescriptor.ReadFrom(extentData, a * 8);

                        if (blocksSeen + extentDescriptor.BlockCount > block)
                        {
                            extentLogicalStart = blocksSeen * (long)this.context.VolumeHeader.BlockSize;
                            return extentDescriptor;
                        }

                        blocksSeen += extentDescriptor.BlockCount;
                    }
                }
                else
                {
                    throw new IOException("Missing extent from extent overflow file: cnid=" + this.cnid + ", blocksSeen=" +
                                          blocksSeen);
                }
            }

            throw new InvalidOperationException("Requested file fragment beyond EOF");
        }
    }
}