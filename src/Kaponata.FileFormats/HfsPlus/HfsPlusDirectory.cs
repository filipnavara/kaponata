// <copyright file="HfsPlusDirectory.cs" company="Kenneth Bell, Quamotion bv">
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
using System;
using System.Collections.Generic;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Represeents a directory entry in the HFS+ file system.
    /// </summary>
    public sealed class HfsPlusDirectory : HfsPlusFile, IVfsDirectory<DirEntry, HfsPlusFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HfsPlusDirectory"/> class.
        /// </summary>
        /// <param name="context">
        /// The HFS+ conext.
        /// </param>
        /// <param name="nodeId">
        /// The <see cref="CatalogNodeId"/> of this directory.
        /// </param>
        /// <param name="fileInfo">
        /// Metadata which describes this directory.
        /// </param>
        public HfsPlusDirectory(Context context, CatalogNodeId nodeId, CommonCatalogFileInfo fileInfo)
            : base(context, nodeId, fileInfo)
        {
        }

        /// <inheritdoc/>
        public ICollection<DirEntry> AllEntries
        {
            get
            {
                List<DirEntry> results = new List<DirEntry>();

                this.Context.Catalog.VisitRange((key, data) =>
                       {
                           if (key.NodeId == this.NodeId)
                           {
                               if (data != null && !string.IsNullOrEmpty(key.Name) && DirEntry.IsFileOrDirectory(data))
                               {
                                   results.Add(new DirEntry(key.Name, data));
                               }

                               return 0;
                           }

                           return key.NodeId < this.NodeId ? -1 : 1;
                       });

                return results;
            }
        }

        /// <inheritdoc/>
        public DirEntry Self
        {
            get
            {
                byte[] dirThreadData = this.Context.Catalog.Find(new CatalogKey(this.NodeId, string.Empty));

                CatalogThread dirThread = new CatalogThread();
                dirThread.ReadFrom(dirThreadData, 0);

                byte[] dirEntryData = this.Context.Catalog.Find(new CatalogKey(dirThread.ParentId, dirThread.Name));

                return new DirEntry(dirThread.Name, dirEntryData);
            }
        }

        /// <inheritdoc/>
        public DirEntry GetEntryByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Attempt to lookup empty file name", nameof(name));
            }

            byte[] dirEntryData = this.Context.Catalog.Find(new CatalogKey(this.NodeId, name));
            if (dirEntryData == null)
            {
                return null;
            }

            return new DirEntry(name, dirEntryData);
        }

        /// <inheritdoc/>
        public DirEntry CreateNewFile(string name)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override string ToString() => this.Self.FileName;
    }
}