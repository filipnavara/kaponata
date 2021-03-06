// <copyright file="HfsPlusFileSystem.cs" company="Kenneth Bell, Quamotion bv">
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

using DiscUtils.Streams;
using DiscUtils.Vfs;
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// Class that interprets Apple's HFS+ file system, found in DMG files.
    /// </summary>
    public class HfsPlusFileSystem : VfsFileSystemFacade, IUnixFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HfsPlusFileSystem"/> class.
        /// </summary>
        /// <param name="stream">A stream containing the file system.</param>
        public HfsPlusFileSystem(Stream stream)
            : base(new HfsPlusFileSystemImpl(stream))
        {
        }

        /// <summary>
        /// Gets the Unix (BSD) file information about a file or directory.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>Unix file information.</returns>
        public UnixFileSystemInfo GetUnixFileInfo(string path)
        {
            return this.GetRealFileSystem<HfsPlusFileSystemImpl>().GetUnixFileInfo(path);
        }

        /// <summary>
        /// Detects whether a <see cref="Stream"/> represents a HFS+ file system or not.
        /// </summary>
        /// <param name="stream">
        /// The stream for which to detect whether it contains a HFS+ file system or not.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="stream"/> contains a HFS+ file system;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool Detect(Stream stream)
        {
            if (stream == null || stream.Length < 1536)
            {
                return false;
            }

            stream.Position = 1024;

            byte[] headerBuf = StreamUtilities.ReadExact(stream, 512);
            VolumeHeader hdr = new VolumeHeader();
            hdr.ReadFrom(headerBuf, 0);

            return hdr.IsValid;
        }
    }
}