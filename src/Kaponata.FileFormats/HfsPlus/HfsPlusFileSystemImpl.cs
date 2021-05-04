// <copyright file="HfsPlusFileSystemImpl.cs" company="Kenneth Bell, Quamotion bv">
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
using DiscUtils.Vfs;
using System;
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// An implementation of the HFS+ file system.
    /// </summary>
    internal sealed class HfsPlusFileSystemImpl : VfsFileSystem<DirEntry, HfsPlusFile, HfsPlusDirectory, Context>, IUnixFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HfsPlusFileSystemImpl"/> class.
        /// </summary>
        /// <param name="stream">A stream containing the file system.</param>
        public HfsPlusFileSystemImpl(Stream stream)
            : base(new DiscFileSystemOptions())
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.Length < 0x600)
            {
                throw new InvalidDataException("The stream does not represent a valid HFS+ file system");
            }

            stream.Position = 1024;

            byte[] headerBuf = StreamUtilities.ReadExact(stream, 512);
            VolumeHeader hdr = new VolumeHeader();
            hdr.ReadFrom(headerBuf, 0);

            if (!hdr.IsValid)
            {
                throw new InvalidDataException("The stream does not represent a valid HFS+ file system");
            }

            this.Context = new Context();
            this.Context.VolumeStream = stream;
            this.Context.VolumeHeader = hdr;

            FileBuffer catalogBuffer = new FileBuffer(this.Context, hdr.CatalogFile, CatalogNodeId.CatalogFileId);
            this.Context.Catalog = new BTree<CatalogKey>(catalogBuffer);

            FileBuffer extentsBuffer = new FileBuffer(this.Context, hdr.ExtentsFile, CatalogNodeId.ExtentsFileId);
            this.Context.ExtentsOverflow = new BTree<ExtentKey>(extentsBuffer);

            FileBuffer attributesBuffer = new FileBuffer(this.Context, hdr.AttributesFile, CatalogNodeId.AttributesFileId);
            this.Context.Attributes = new BTree<AttributeKey>(attributesBuffer);

            // Establish Root directory
            byte[] rootThreadData = this.Context.Catalog.Find(new CatalogKey(CatalogNodeId.RootFolderId, string.Empty));
            CatalogThread rootThread = new CatalogThread();
            rootThread.ReadFrom(rootThreadData, 0);
            byte[] rootDirEntryData = this.Context.Catalog.Find(new CatalogKey(rootThread.ParentId, rootThread.Name));
            DirEntry rootDirEntry = new DirEntry(rootThread.Name, rootDirEntryData);
            this.RootDirectory = (HfsPlusDirectory)this.GetFile(rootDirEntry);
        }

        /// <inheritdoc/>
        public override string FriendlyName
        {
            get { return "Apple HFS+"; }
        }

        /// <inheritdoc/>
        public override string VolumeLabel
        {
            get
            {
                byte[] rootThreadData = this.Context.Catalog.Find(new CatalogKey(CatalogNodeId.RootFolderId, string.Empty));
                CatalogThread rootThread = new CatalogThread();
                rootThread.ReadFrom(rootThreadData, 0);

                return rootThread.Name;
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override long Size
        {
            get { throw new NotSupportedException("Filesystem size is not (yet) supported"); }
        }

        /// <inheritdoc/>
        public override long UsedSpace
        {
            get { throw new NotSupportedException("Filesystem size is not (yet) supported"); }
        }

        /// <inheritdoc/>
        public override long AvailableSpace
        {
            get { throw new NotSupportedException("Filesystem size is not (yet) supported"); }
        }

        /// <inheritdoc/>
        public UnixFileSystemInfo GetUnixFileInfo(string path)
        {
            DirEntry dirEntry = this.GetDirectoryEntry(path);
            if (dirEntry == null)
            {
                throw new FileNotFoundException("No such file or directory", path);
            }

            return dirEntry.CatalogFileInfo.FileSystemInfo;
        }

        /// <inheritdoc/>
        protected override HfsPlusFile ConvertDirEntryToFile(DirEntry dirEntry)
        {
            if (dirEntry.IsDirectory)
            {
                return new HfsPlusDirectory(this.Context, dirEntry.NodeId, dirEntry.CatalogFileInfo);
            }

            if (dirEntry.IsSymlink)
            {
                return new Symlink(this.Context, dirEntry.NodeId, dirEntry.CatalogFileInfo);
            }

            return new HfsPlusFile(this.Context, dirEntry.NodeId, dirEntry.CatalogFileInfo);
        }
    }
}
