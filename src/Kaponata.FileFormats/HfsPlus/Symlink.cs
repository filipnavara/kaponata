// <copyright file="Symlink.cs" company="Quamotion bv">
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
using DiscUtils.Vfs;
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A symbolic link.
    /// </summary>
    public class Symlink : HfsPlusFile, IVfsSymlink<DirEntry, HfsPlusFile>
    {
        private string targetPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Symlink"/> class.
        /// </summary>
        /// <param name="context">
        /// The HFS+ context to which the symbolic link is tied.
        /// </param>
        /// <param name="nodeId">
        /// The <see cref="CatalogNodeId"/> of the symblolic link file.
        /// </param>
        /// <param name="catalogInfo">
        /// File metadata.
        /// </param>
        public Symlink(Context context, CatalogNodeId nodeId, CommonCatalogFileInfo catalogInfo)
            : base(context, nodeId, catalogInfo)
        {
        }

        /// <inheritdoc/>
        public string TargetPath
        {
            get
            {
                if (this.targetPath == null)
                {
                    using (BufferStream stream = new BufferStream(this.FileContent, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        this.targetPath = reader.ReadToEnd();
                        this.targetPath = this.targetPath.Replace('/', '\\');
                    }
                }

                return this.targetPath;
            }
        }
    }
}