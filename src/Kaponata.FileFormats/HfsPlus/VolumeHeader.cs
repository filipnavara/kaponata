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
    internal sealed class VolumeHeader : IByteArraySerializable
    {
        public const ushort HfsPlusSignature = 0x482b;

        public ForkData AllocationFile;
        public VolumeAttributes Attributes;
        public ForkData AttributesFile;
        public DateTime BackupDate;

        public uint BlockSize;
        public ForkData CatalogFile;
        public DateTime CheckedDate;

        public DateTime CreateDate;
        public uint DataClumpSize;
        public ulong EncodingsBitmap;
        public ForkData ExtentsFile;

        public uint FileCount;

        public uint[] FinderInfo;
        public uint FolderCount;
        public uint FreeBlocks;
        public uint JournalInfoBlock;
        public uint LastMountedVersion;
        public DateTime ModifyDate;

        public uint NextAllocation;
        public CatalogNodeId NextCatalogId;
        public uint ResourceClumpSize;

        public ushort Signature;
        public ForkData StartupFile;
        public uint TotalBlocks;
        public ushort Version;

        public uint WriteCount;

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
            if (!this.IsValid) return this.Size;

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