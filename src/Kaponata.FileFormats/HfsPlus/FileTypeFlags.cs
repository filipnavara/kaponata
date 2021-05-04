// <copyright file="FileTypeFlags.cs" company="Quamotion bv">
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
    /// Describes the type of file.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#HardLinks"/>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#SymbolicLinks"/>
    [Flags]
    internal enum FileTypeFlags
    {
        /// <summary>
        /// The file is a regular file.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The file is a symbolic link.
        /// </summary>
        SymLinkFileType = 0x736C6E6B, /* 'slnk' */

        /// <summary>
        /// The file is a symbolic link creator.
        /// </summary>
        SymLinkCreator = 0x72686170, /* 'rhap' */

        /// <summary>
        /// The file is a hard link.
        /// </summary>
        HardLinkFileType = 0x686C6E6B, /* 'hlnk' */

        /// <summary>
        /// The file is a HFS+ creator.
        /// </summary>
        HFSPlusCreator = 0x6866732B, /* 'hfs+' */
    }
}