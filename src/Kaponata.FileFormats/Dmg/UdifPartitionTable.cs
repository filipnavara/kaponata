// <copyright file="UdifPartitionTable.cs" company="Quamotion bv">
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

using DiscUtils.Partitions;
using System;
using System.Collections.ObjectModel;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// A <see cref="PartitionTable"/> which is read from a DMG file.
    /// </summary>
    internal class UdifPartitionTable : PartitionTable
    {
        private readonly UdifBuffer buffer;
        private readonly Disk disk;
        private readonly Collection<PartitionInfo> partitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdifPartitionTable"/> class.
        /// </summary>
        /// <param name="disk">
        /// The disk to which the partition table belongs.
        /// </param>
        /// <param name="buffer">
        /// A <see cref="UdifBuffer"/> which supports reading data from the disk.
        /// </param>
        public UdifPartitionTable(Disk disk, UdifBuffer buffer)
        {
            this.buffer = buffer;
            this.partitions = new Collection<PartitionInfo>();
            this.disk = disk;

            foreach (CompressedBlock block in this.buffer.Blocks)
            {
                UdifPartitionInfo partition = new UdifPartitionInfo(this.disk, block);
                this.partitions.Add(partition);
            }
        }

        /// <inheritdoc/>
        public override Guid DiskGuid
        {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Gets the partitions present on the disk.
        /// </summary>
        public override ReadOnlyCollection<PartitionInfo> Partitions
        {
            get { return new ReadOnlyCollection<PartitionInfo>(this.partitions); }
        }

        /// <inheritdoc/>
        public override void Delete(int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int CreateAligned(long size, WellKnownPartitionType type, bool active, int alignment)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int Create(long size, WellKnownPartitionType type, bool active)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int CreateAligned(WellKnownPartitionType type, bool active, int alignment)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int Create(WellKnownPartitionType type, bool active)
        {
            throw new NotImplementedException();
        }
    }
}