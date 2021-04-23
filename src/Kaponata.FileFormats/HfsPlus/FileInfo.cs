// <copyright file="FileInfo.cs" company="Kenneth Bell, Quamotion bv">
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
    /// Describes additional file metadata, as used by Finder.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#FinderInfo"/>
    internal class FileInfo : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the file's creator.
        /// </summary>
        public uint FileCreator { get; set; }

        /// <summary>
        /// Gets or sets the type of the file.
        /// </summary>
        public uint FileType { get; set; }

        /// <summary>
        /// Gets or sets flags used by Finder.
        /// </summary>
        public FinderFlags FinderFlags { get; set; }

        /// <summary>
        /// Gets or sets the file's location in the folder.
        /// </summary>
        public Point Point { get; set; }

        /// <inheritdoc/>
        public int Size
        {
            get { return 16; }
        }

        /// <inheritdoc/>
        public int ReadFrom(byte[] buffer, int offset)
        {
            this.FileType = EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            this.FileCreator = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.FinderFlags = (FinderFlags)EndianUtilities.ToUInt16BigEndian(buffer, offset + 8);
            this.Point = EndianUtilities.ToStruct<Point>(buffer, offset + 10);

            return 16;
        }

        /// <inheritdoc/>
        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}