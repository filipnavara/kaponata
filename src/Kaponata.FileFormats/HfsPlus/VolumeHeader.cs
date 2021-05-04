// <copyright file="VolumeHeader.cs" company="Kenneth Bell, Quamotion bv">
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
    /// Represents the HFS+ volume header. The volume header contains information about the volume as a whole,
    /// including the location of other key structures in the volume.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#VolumeHeader"/>
    public sealed class VolumeHeader : IByteArraySerializable
    {
        /// <summary>
        /// The signature of the HFS+ volume header.
        /// </summary>
        public const ushort HfsPlusSignature = 0x482b;

        /// <summary>
        /// Gets or sets information about the location and size of the allocation file.
        /// </summary>
        public ForkData AllocationFile { get; set; }

        /// <summary>
        /// Gets or sets the volume attributes.
        /// </summary>
        public VolumeAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets information about the location and size of the attributes file.
        /// </summary>
        public ForkData AttributesFile { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the volume was last backed up.
        /// </summary>
        public DateTime BackupDate { get; set; }

        /// <summary>
        /// Gets or sets the allocation block size, in bytes.
        /// </summary>
        public uint BlockSize { get; set; }

        /// <summary>
        /// Gets or sets information about the location and size of the catalog file.
        /// </summary>
        public ForkData CatalogFile { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the volume was last checked for consistency.
        /// </summary>
        public DateTime CheckedDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the volume was created.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the default clump size for data forks, in bytes.
        /// </summary>
        public uint DataClumpSize { get; set; }

        /// <summary>
        /// Gets or sets the text encodings used in the file and folder names on the volume.
        /// </summary>
        public ulong EncodingsBitmap { get; set; }

        /// <summary>
        /// Gets or sets information about the location and size of the extents file.
        /// </summary>
        public ForkData ExtentsFile { get; set; }

        /// <summary>
        /// Gets or sets the total number of files on the volume.
        /// </summary>
        public uint FileCount { get; set; }

        /// <summary>
        /// Gets or sets an array of 32-bit items contains information used by the Mac OS Finder, and the system software boot process.
        /// </summary>
        public uint[] FinderInfo { get; set; }

        /// <summary>
        /// Gets or sets the total number of folders on the volume.
        /// </summary>
        public uint FolderCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of unused allocation blocks on the disk.
        /// </summary>
        public uint FreeBlocks { get; set; }

        /// <summary>
        /// Gets or sets the allocation block number of the allocation block which contains the JournalInfoBlock for this volume's journal.
        /// This field is valid only if bit kHFSVolumeJournaledBit is set in the attribute field; otherwise, this field is reserved.
        /// </summary>
        public uint JournalInfoBlock { get; set; }

        /// <summary>
        /// Gets or sets a value which uniquely identifies the implementation that last mounted this volume for writing.
        /// </summary>
        public uint LastMountedVersion { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the volume was last modified.
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// Gets or sets the start of next allocation search.
        /// </summary>
        public uint NextAllocation { get; set; }

        /// <summary>
        /// Gets or sets the next unused catalog ID.
        /// </summary>
        public CatalogNodeId NextCatalogId { get; set; }

        /// <summary>
        /// Gets or sets the default clump size for resource forks, in bytes.
        /// </summary>
        public uint ResourceClumpSize { get; set; }

        /// <summary>
        /// Gets or sets the volume signature.
        /// </summary>
        public ushort Signature { get; set; }

        /// <summary>
        /// Gets or sets information about the location and size of the startup file.
        /// </summary>
        public ForkData StartupFile { get; set; }

        /// <summary>
        /// Gets or sets the total number of allocation blocks on the disk.
        /// </summary>
        public uint TotalBlocks { get; set; }

        /// <summary>
        /// Gets or sets the version of the volume format, which is currently 4 (<c>kHFSPlusVersion</c>)
        /// for HFS Plus volumes, or 5 (<c>kHFSXVersion</c>) for HFSX volumes.
        /// </summary>
        public ushort Version { get; set; }

        /// <summary>
        /// Gets or sets a value which is incremented every time a volume is mounted.
        /// </summary>
        public uint WriteCount { get; set; }

        /// <summary>
        /// Gets a value indicating whether the volume header is valid.
        /// </summary>
        public bool IsValid
        {
            get { return this.Signature == HfsPlusSignature; }
        }

        /// <inheritdoc/>
        public int Size
        {
            get { return 512; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.Signature = EndianUtilities.ToUInt16BigEndian(buffer, offset + 0);
            if (!this.IsValid)
            {
                return this.Size;
            }

            this.Version = EndianUtilities.ToUInt16BigEndian(buffer, offset + 2);
            this.Attributes = (VolumeAttributes)EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.LastMountedVersion = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.JournalInfoBlock = EndianUtilities.ToUInt32BigEndian(buffer, offset + 12);

            this.CreateDate = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Local, buffer, offset + 16);
            this.ModifyDate = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 20);
            this.BackupDate = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 24);
            this.CheckedDate = HfsPlusUtilities.ReadHFSPlusDate(DateTimeKind.Utc, buffer, offset + 28);

            this.FileCount = EndianUtilities.ToUInt32BigEndian(buffer, offset + 32);
            this.FolderCount = EndianUtilities.ToUInt32BigEndian(buffer, offset + 36);

            this.BlockSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 40);
            this.TotalBlocks = EndianUtilities.ToUInt32BigEndian(buffer, offset + 44);
            this.FreeBlocks = EndianUtilities.ToUInt32BigEndian(buffer, offset + 48);

            this.NextAllocation = EndianUtilities.ToUInt32BigEndian(buffer, offset + 52);
            this.ResourceClumpSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 56);
            this.DataClumpSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 60);
            this.NextCatalogId = new CatalogNodeId(EndianUtilities.ToUInt32BigEndian(buffer, offset + 64));

            this.WriteCount = EndianUtilities.ToUInt32BigEndian(buffer, offset + 68);
            this.EncodingsBitmap = EndianUtilities.ToUInt64BigEndian(buffer, offset + 72);

            this.FinderInfo = new uint[8];
            for (int i = 0; i < 8; ++i)
            {
                this.FinderInfo[i] = EndianUtilities.ToUInt32BigEndian(buffer, offset + 80 + (i * 4));
            }

            this.AllocationFile = EndianUtilities.ToStruct<ForkData>(buffer, offset + 112);
            this.ExtentsFile = EndianUtilities.ToStruct<ForkData>(buffer, offset + 192);
            this.CatalogFile = EndianUtilities.ToStruct<ForkData>(buffer, offset + 272);
            this.AttributesFile = EndianUtilities.ToStruct<ForkData>(buffer, offset + 352);
            this.StartupFile = EndianUtilities.ToStruct<ForkData>(buffer, offset + 432);

            return 512;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}