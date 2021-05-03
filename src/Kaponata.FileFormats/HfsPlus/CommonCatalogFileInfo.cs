// <copyright file="CommonCatalogFileInfo.cs" company="Kenneth Bell, Quamotion bv">
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

using DiscUtils.Streams;
using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Common descriptors for file and directory records.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#CatalogFile"/>
    public abstract class CommonCatalogFileInfo : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the date and time the file or folder's contents were last read.
        /// </summary>
        public DateTime AccessTime { get; set; }

        /// <summary>
        /// Gets or sets the last date and time that any field in the folder's catalog record was changed.
        /// </summary>
        public DateTime AttributeModifyTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the folder was last backed up.
        /// </summary>
        public DateTime BackupTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the file or folder's contents were last changed.
        /// </summary>
        public DateTime ContentModifyTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the file or folder was created.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CatalogNodeId"/> of this file or folder.
        /// </summary>
        public CatalogNodeId FileId { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="UnixFileSystemInfo"/> object which describes common attributes of the
        /// file or folder.
        /// </summary>
        public UnixFileSystemInfo FileSystemInfo { get; set; }

        /// <summary>
        /// Gets or sets the catalog data record type.
        /// </summary>
        public CatalogRecordType RecordType { get; set; }

        /// <summary>
        /// Gets or sets additional file system information.
        /// </summary>
        public uint UnixSpecialField { get; set; }

        /// <inheritdoc/>
        public abstract int Size { get; }

        /// <inheritdoc/>
        public virtual int ReadFrom(byte[] buffer, int offset)
        {
            this.RecordType = (CatalogRecordType)EndianUtilities.ToInt16BigEndian(buffer, offset + 0);
            this.FileId = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.CreateTime = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 12);
            this.ContentModifyTime = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 16);
            this.AttributeModifyTime = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 20);
            this.AccessTime = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 24);
            this.BackupTime = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 28);

            uint special;
            this.FileSystemInfo = HfsPlusUtilities.ReadBsdInfo(buffer, offset + 32, out special);
            this.UnixSpecialField = special;

            return 46;
        }

        /// <inheritdoc/>
        public abstract void WriteTo(byte[] buffer, int offset);
    }
}