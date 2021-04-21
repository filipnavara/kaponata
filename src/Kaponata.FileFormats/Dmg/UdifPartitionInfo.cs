// <copyright file="UdifPartitionInfo.cs" company="Quamotion bv">
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
//

#nullable disable

using DiscUtils.Partitions;
using DiscUtils.Streams;
using System;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Provides information about an individual partition in a DMG file.
    /// </summary>
    internal class UdifPartitionInfo : PartitionInfo
    {
        private readonly CompressedBlock block;
        private readonly Disk disk;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdifPartitionInfo"/> class.
        /// </summary>
        /// <param name="disk">
        /// The disk to which the partition belongs.
        /// </param>
        /// <param name="block">
        /// A <see cref="CompressedBlock"/> which represents the individual partition.
        /// </param>
        public UdifPartitionInfo(Disk disk, CompressedBlock block)
        {
            this.block = block;
            this.disk = disk;
        }

        /// <inheritdoc/>
        public override byte BiosType
        {
            get { return 0; }
        }

        /// <inheritdoc/>
        public override long FirstSector
        {
            get { return this.block.FirstSector; }
        }

        /// <inheritdoc/>
        public override Guid GuidType
        {
            get { return Guid.Empty; }
        }

        /// <inheritdoc/>
        public override long LastSector
        {
            get { return this.block.FirstSector + this.block.SectorCount; }
        }

        /// <inheritdoc/>
        public override long SectorCount
        {
            get { return this.block.SectorCount; }
        }

        /// <inheritdoc/>
        public override string TypeAsString
        {
            get { return this.GetType().FullName; }
        }

        /// <inheritdoc/>
        public override PhysicalVolumeType VolumeType
        {
            get { return PhysicalVolumeType.ApplePartition; }
        }

        /// <inheritdoc/>
        public override SparseStream Open()
        {
            return new SubStream(this.disk.Content, this.FirstSector * this.disk.SectorSize, this.SectorCount * this.disk.SectorSize);
        }
    }
}