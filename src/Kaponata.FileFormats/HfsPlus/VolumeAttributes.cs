// <copyright file="VolumeAttributes.cs" company="Kenneth Bell, Quamotion bv">
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

using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Additional volume attributes.
    /// </summary>
    [Flags]
    internal enum VolumeAttributes : uint
    {
        /// <summary>
        /// No bits are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// This bit is set when the volume is write-protected due to some hardware setting.
        /// </summary>
        VolumeHardwareLock = 0x00000080,

        /// <summary>
        /// This bit is set if the volume was correctly flushed before being unmounted or ejected.
        /// </summary>
        VolumeUnmounted = 0x00000100,

        /// <summary>
        /// This bit is set if there are any records in the extents overflow file for bad blocks.
        /// </summary>
        VolumeSparedBlocks = 0x00000200,

        /// <summary>
        /// This bit is set if the blocks from this volume should not be cached.
        /// </summary>
        VolumeNoCacheRequired = 0x00000400,

        /// <summary>
        /// This bit is similar to <see cref="VolumeUnmounted"/>, but inverted in meaning.
        /// </summary>
        BootVolumeInconsistent = 0x00000800,

        /// <summary>
        /// This bit is set when the nextCatalogID field overflows 32 bits, forcing smaller catalog node IDs to be reused.
        /// </summary>
        CatalogNodeIdsReused = 0x00001000,

        /// <summary>
        /// If this bit is set, the volume has a journal, which can be located using the <see cref="VolumeHeader.JournalInfoBlock"/> field.
        /// </summary>
        VolumeJournaled = 0x00002000,

        /// <summary>
        /// This bit is set if the volume is write-protected due to a software setting.
        /// </summary>
        VolumeSoftwareLock = 0x00008000,
    }
}