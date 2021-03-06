// <copyright file="Context.cs" company="Kenneth Bell, Quamotion bv">
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

using DiscUtils.Vfs;
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Contains the basic information of a HFS+ file system.
    /// </summary>
    public sealed class Context : VfsContext
    {
        /// <summary>
        /// Gets or sets the B-tree which represents the HFS+ Attributes File.
        /// </summary>
        public BTree<AttributeKey> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the B-tree which represents the HFS+ Catalog File.
        /// </summary>
        public BTree<CatalogKey> Catalog { get; set; }

        /// <summary>
        /// Gets or sets the B-tree which represents the HFS+ Extents Overflow File.
        /// </summary>
        public BTree<ExtentKey> ExtentsOverflow { get; set; }

        /// <summary>
        /// Gets or sets the HFS+ volume header.
        /// </summary>
        public VolumeHeader VolumeHeader { get; set; }

        /// <summary>
        /// Gets or sets the raw <see cref="Stream"/> which provides access to the HFS+ volume.
        /// </summary>
        public Stream VolumeStream { get; set; }
    }
}