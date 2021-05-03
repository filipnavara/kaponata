// <copyright file="FinderFlags.cs" company="Kenneth Bell, Quamotion bv">
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
    /// File flags used by Finder.
    /// </summary>
    [Flags]
    public enum FinderFlags : ushort
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Unused and reserved.
        /// </summary>
        IsOnDesk = 0x0001,

        /// <summary>
        /// Three bits of color coding
        /// </summary>
        Color = 0x000E,

        /// <summary>
        /// If clear, the application needs to write to its resource fork,
        /// and therefore cannot be shared on a server
        /// </summary>
        IsShared = 0x0040,

        /// <summary>
        /// This file contains no INIT resource.
        /// </summary>
        HasNoInits = 0x0080,

        /// <summary>
        /// Clear if the file contains desktop database resources
        /// ('BNDL', 'FREF', 'open', 'kind'...) that have not been added yet. Set
        /// only by the Finder.
        /// </summary>
        HasBeenInitied = 0x0100,

        /// <summary>
        /// The file or directory has a customized icon.
        /// </summary>
        HasCustomIcon = 0x0400,

        /// <summary>
        /// The file is a stationery pad.
        /// </summary>
        IsStationary = 0x0800,

        /// <summary>
        /// The file or directory can't be renamed from Finder,
        /// and its icon can't be changed.
        /// </summary>
        NameLocked = 0x1000,

        /// <summary>
        /// The file has a bundle resource.
        /// </summary>
        HasBundle = 0x2000,

        /// <summary>
        /// The file or directory is invisible from Finder and
        /// from Standard File Package dialog boxes.
        /// </summary>
        IsInvisible = 0x4000,

        /// <summary>
        /// The file is an alias file.
        /// </summary>
        IsAlias = 0x8000,
    }
}