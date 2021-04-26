// <copyright file="CatalogNodeId.cs" company="Kenneth Bell, Quamotion bv">
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

using System.Globalization;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represents the HFS+ Catalog Node ID. Each file or folder in the catalog file is assigned a unique
    /// catalog node ID (CNID).
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#CNID"/>
    public struct CatalogNodeId
    {
        /// <summary>
        /// Parent ID of the root folder.
        /// </summary>
        public static readonly CatalogNodeId RootParentId = new CatalogNodeId(1);

        /// <summary>
        /// Folder ID of the root folder.
        /// </summary>
        public static readonly CatalogNodeId RootFolderId = new CatalogNodeId(2);

        /// <summary>
        /// File ID of the extents overflow file.
        /// </summary>
        public static readonly CatalogNodeId ExtentsFileId = new CatalogNodeId(3);

        /// <summary>
        /// File ID of the catalog file.
        /// </summary>
        public static readonly CatalogNodeId CatalogFileId = new CatalogNodeId(4);

        /// <summary>
        /// File ID of the bad block file. The bad block file is not a file in the same sense as a
        /// special file and a user file.
        /// </summary>
        public static readonly CatalogNodeId BadBlockFileId = new CatalogNodeId(5);

        /// <summary>
        /// File ID of the allocation file (introduced with HFS Plus).
        /// </summary>
        public static readonly CatalogNodeId AllocationFileId = new CatalogNodeId(6);

        /// <summary>
        /// File ID of the startup file (introduced with HFS Plus).
        /// </summary>
        public static readonly CatalogNodeId StartupFileId = new CatalogNodeId(7);

        /// <summary>
        /// File ID of the attributes file (introduced with HFS Plus).
        /// </summary>
        public static readonly CatalogNodeId AttributesFileId = new CatalogNodeId(8);

        /// <summary>
        /// Used temporarily by <c>fsck_hfs</c> when rebuilding the catalog file.
        /// </summary>
        public static readonly CatalogNodeId RepairCatalogFileId = new CatalogNodeId(14);

        /// <summary>
        /// Used temporarily during <c>ExchangeFiles</c> operations.
        /// </summary>
        public static readonly CatalogNodeId BogusExtentFileId = new CatalogNodeId(15);

        /// <summary>
        /// First CNID available for use by user files and folders.
        /// </summary>
        public static readonly CatalogNodeId FirstUserCatalogNodeId = new CatalogNodeId(16);

        private readonly uint id;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogNodeId"/> struct.
        /// </summary>
        /// <param name="id">
        /// The raw catalog node id.
        /// </param>
        public CatalogNodeId(uint id)
        {
            this.id = id;
        }

        public static implicit operator uint(CatalogNodeId nodeId)
        {
            return nodeId.id;
        }

        public static implicit operator CatalogNodeId(uint id)
        {
            return new CatalogNodeId(id);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.id.ToString(CultureInfo.InvariantCulture);
        }
    }
}