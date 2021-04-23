// <copyright file="DirEntry.cs" company="Kenneth Bell, Quamotion bv">
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

using DiscUtils.Internal;
using DiscUtils.Streams;
using DiscUtils.Vfs;
using System;
using System.IO;

namespace DiscUtils.HfsPlus
{
    internal sealed class DirEntry : VfsDirEntry
    {
        public DirEntry(string name, byte[] dirEntryData)
        {
            this.FileName = name;
            this.CatalogFileInfo = ParseDirEntryData(dirEntryData);
        }

        public CommonCatalogFileInfo CatalogFileInfo { get; }

        /// <inheritdoc/>
        public override DateTime CreationTimeUtc
        {
            get { return this.CatalogFileInfo.CreateTime; }
        }

        /// <inheritdoc/>
        public override FileAttributes FileAttributes
        {
            get { return Utilities.FileAttributesFromUnixFileType(this.CatalogFileInfo.FileSystemInfo.FileType); }
        }

        /// <inheritdoc/>
        public override string FileName { get; }

        /// <inheritdoc/>
        public override bool HasVfsFileAttributes
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool HasVfsTimeInfo
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool IsDirectory
        {
            get { return this.CatalogFileInfo.RecordType == CatalogRecordType.FolderRecord; }
        }

        /// <inheritdoc/>
        public override bool IsSymlink
        {
            get
            {
                return
                    !this.IsDirectory
                    && (FileTypeFlags)((CatalogFileInfo)this.CatalogFileInfo).FileInfo.FileType == FileTypeFlags.SymLinkFileType;
            }
        }

        /// <inheritdoc/>
        public override DateTime LastAccessTimeUtc
        {
            get { return this.CatalogFileInfo.AccessTime; }
        }

        /// <inheritdoc/>
        public override DateTime LastWriteTimeUtc
        {
            get { return this.CatalogFileInfo.ContentModifyTime; }
        }

        public CatalogNodeId NodeId
        {
            get { return this.CatalogFileInfo.FileId; }
        }

        /// <inheritdoc/>
        public override long UniqueCacheId
        {
            get { return this.CatalogFileInfo.FileId; }
        }

        internal static bool IsFileOrDirectory(byte[] dirEntryData)
        {
            CatalogRecordType type = (CatalogRecordType)EndianUtilities.ToInt16BigEndian(dirEntryData, 0);
            return type == CatalogRecordType.FolderRecord || type == CatalogRecordType.FileRecord;
        }

        private static CommonCatalogFileInfo ParseDirEntryData(byte[] dirEntryData)
        {
            CatalogRecordType type = (CatalogRecordType)EndianUtilities.ToInt16BigEndian(dirEntryData, 0);

            CommonCatalogFileInfo result = null;
            switch (type)
            {
                case CatalogRecordType.FolderRecord:
                    result = new CatalogDirInfo();
                    break;
                case CatalogRecordType.FileRecord:
                    result = new CatalogFileInfo();
                    break;
                default:
                    throw new NotImplementedException("Unknown catalog record type: " + type);
            }

            result.ReadFrom(dirEntryData, 0);
            return result;
        }
    }
}