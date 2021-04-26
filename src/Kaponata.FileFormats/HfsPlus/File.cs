// <copyright file="File.cs" company="Kenneth Bell, Quamotion bv">
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

#nullable disable

using DiscUtils.Compression;
using DiscUtils.Streams;
using DiscUtils.Vfs;
using System;
using System.IO;
using System.IO.Compression;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents a file in a HFS+ file system.
    /// </summary>
    internal class File : IVfsFileWithStreams
    {
        private const string CompressionAttributeName = "com.apple.decmpfs";
        private readonly CommonCatalogFileInfo catalogInfo;
        private readonly bool hasCompressionAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="File"/> class.
        /// </summary>
        /// <param name="context">
        /// The HFS+ context to which the file belongs.
        /// </param>
        /// <param name="nodeId">
        /// The <see cref="CatalogNodeId"/> of the file.
        /// </param>
        /// <param name="catalogInfo">
        /// Additional file metadata.
        /// </param>
        public File(Context context, CatalogNodeId nodeId, CommonCatalogFileInfo catalogInfo)
        {
            this.Context = context;
            this.NodeId = nodeId;
            this.catalogInfo = catalogInfo;
            this.hasCompressionAttribute =
                this.Context.Attributes.Find(new AttributeKey(this.NodeId, CompressionAttributeName)) != null;
        }

        /// <inheritdoc/>
        public DateTime LastAccessTimeUtc
        {
            get { return this.catalogInfo.AccessTime; }

            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public DateTime LastWriteTimeUtc
        {
            get { return this.catalogInfo.ContentModifyTime; }

            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public DateTime CreationTimeUtc
        {
            get { return this.catalogInfo.CreateTime; }

            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public FileAttributes FileAttributes
        {
            get { return HfsPlusUtilities.FileAttributesFromUnixFileType(this.catalogInfo.FileSystemInfo.FileType); }

            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public long FileLength
        {
            get
            {
                CatalogFileInfo fileInfo = this.catalogInfo as CatalogFileInfo;
                if (fileInfo == null)
                {
                    throw new InvalidOperationException();
                }

                return (long)fileInfo.DataFork.LogicalSize;
            }
        }

        /// <inheritdoc/>
        public IBuffer FileContent
        {
            get
            {
                CatalogFileInfo fileInfo = this.catalogInfo as CatalogFileInfo;
                if (fileInfo == null)
                {
                    throw new InvalidOperationException();
                }

                if (this.hasCompressionAttribute)
                {
                    // Open the compression attribute
                    byte[] compressionData =
                        this.Context.Attributes.Find(new AttributeKey(this.catalogInfo.FileId, "com.apple.decmpfs"));

                    var inlineAttributeHeader = new AttributeInlineData();
                    inlineAttributeHeader.ReadFrom(compressionData, 0);

                    var compressionHeader = new CompressionAttribute();
                    compressionHeader.ReadFrom(compressionData, inlineAttributeHeader.Size);

                    var compressedDataOffset = inlineAttributeHeader.Size + compressionHeader.Size;

                    // There are multiple possibilities, not all of which are supported by DiscUtils.HfsPlus.
                    // See FileCompressionType for a full description of all possibilities.
                    switch (compressionHeader.CompressionType)
                    {
                        case FileCompressionType.ZlibAttribute:
                            if ((uint)compressionHeader.UncompressedSize == compressedDataOffset - 0x11)
                            {
                                // Inline, no compression, very small file
                                MemoryStream stream = new MemoryStream(
                                    compressionData,
                                    compressedDataOffset + 1,
                                    (int)compressionHeader.UncompressedSize,
                                    false);

                                return new StreamBuffer(stream, Ownership.Dispose);
                            }
                            else
                            {
                                // Inline, but we must decompress
                                MemoryStream stream = new MemoryStream(
                                    compressionData,
                                    compressedDataOffset,
                                    compressionData.Length - compressedDataOffset,
                                    false);

                                // The usage upstream will want to seek or set the position, the ZlibBuffer
                                // wraps around a zlibstream and allows for this (in a limited fashion).
                                ZlibStream compressedStream = new ZlibStream(stream, CompressionMode.Decompress, false);
                                return new ZlibBuffer(compressedStream, Ownership.Dispose);
                            }

                        case FileCompressionType.ZlibResource:
                            // The data is stored in the resource fork.
                            FileBuffer buffer = new FileBuffer(this.Context, fileInfo.ResourceFork, fileInfo.FileId);
                            CompressionResourceHeader compressionFork = new CompressionResourceHeader();
                            byte[] compressionForkData = new byte[CompressionResourceHeader.Size];
                            buffer.Read(0, compressionForkData, 0, CompressionResourceHeader.Size);
                            compressionFork.ReadFrom(compressionForkData, 0);

                            // The data is compressed in a number of blocks. Each block originally accounted for
                            // 0x10000 bytes (that's 64 KB) of data. The compressed size may vary.
                            // The data in each block can be read using a SparseStream. The first block contains
                            // the zlib header but the others don't, so we read them directly as deflate streams.
                            // For each block, we create a separate stream which we later aggregate.
                            CompressionResourceBlockHead blockHeader = new CompressionResourceBlockHead();
                            byte[] blockHeaderData = new byte[CompressionResourceBlockHead.Size];
                            buffer.Read(compressionFork.HeaderSize, blockHeaderData, 0, CompressionResourceBlockHead.Size);
                            blockHeader.ReadFrom(blockHeaderData, 0);

                            uint blockCount = blockHeader.NumBlocks;
                            CompressionResourceBlock[] blocks = new CompressionResourceBlock[blockCount];
                            SparseStream[] streams = new SparseStream[blockCount];

                            for (int i = 0; i < blockCount; i++)
                            {
                                // Read the block data, first into a buffer and the into the class.
                                blocks[i] = new CompressionResourceBlock();
                                byte[] blockData = new byte[CompressionResourceBlock.Size];
                                buffer.Read(
                                    compressionFork.HeaderSize + CompressionResourceBlockHead.Size +
                                    (i * CompressionResourceBlock.Size),
                                    blockData,
                                    0,
                                    blockData.Length);
                                blocks[i].ReadFrom(blockData, 0);

                                // Create a SubBuffer which points to the data window that corresponds to the block.
                                SubBuffer subBuffer = new SubBuffer(
                                    buffer,
                                    compressionFork.HeaderSize + blocks[i].Offset + 6,
                                    blocks[i].DataSize);

                                // ... convert it to a stream
                                BufferStream stream = new BufferStream(subBuffer, FileAccess.Read);

                                // ... and create a deflate stream. Because we will concatenate the streams, the streams
                                // must report on their size. We know the size (0x10000) so we pass it as a parameter.
                                DeflateStream s = new SizedDeflateStream(stream, CompressionMode.Decompress, false, 0x10000);
                                streams[i] = SparseStream.FromStream(s, Ownership.Dispose);
                            }

                            // Finally, concatenate the streams together and that's about it.
                            ConcatStream concatStream = new ConcatStream(Ownership.Dispose, streams);
                            return new ZlibBuffer(concatStream, Ownership.Dispose);

                        case FileCompressionType.RawAttribute:
                            // Inline, no compression, very small file
                            return new StreamBuffer(
                                new MemoryStream(
                                    compressionData,
                                    compressedDataOffset + 1,
                                    (int)compressionHeader.UncompressedSize,
                                    false),
                                Ownership.Dispose);

                        default:
                            throw new NotSupportedException($"The HfsPlus compression type {compressionHeader.CompressionType} is not supported by DiscUtils.HfsPlus");
                    }
                }

                return new FileBuffer(this.Context, fileInfo.DataFork, fileInfo.FileId);
            }
        }

        /// <summary>
        /// Gets the HFS+ context to which the file belongs.
        /// </summary>
        protected Context Context { get; }

        /// <summary>
        /// Gets the <see cref="CatalogNodeId"/> of the file.
        /// </summary>
        protected CatalogNodeId NodeId { get; }

        /// <inheritdoc/>
        public SparseStream CreateStream(string name)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public SparseStream OpenExistingStream(string name)
        {
            throw new NotImplementedException();
        }
    }
}