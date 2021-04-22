// <copyright file="RunType.cs" company="Kenneth Bell, Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// Copyright (c) 2008-2011, Kenneth Bell
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

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Determines the approach used to store data in a <see cref="CompressedRun"/>.
    /// </summary>
    public enum RunType : uint
    {
        /// <summary>
        /// Data is zero-filled.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// The data is not compressed.
        /// </summary>
        Raw = 0x00000001,

        /// <summary>
        /// The data is zero-filled.
        /// </summary>
        Zeros = 0x00000002,

        /// <summary>
        /// The data is compressed using Apple Data Compression (ADC).
        /// </summary>
        AdcCompressed = 0x80000004,

        /// <summary>
        /// The data is compressed using zlib data compressed.
        /// </summary>
        ZlibCompressed = 0x80000005,

        /// <summary>
        /// The data is compressed uzing bz2lib data compression.
        /// </summary>
        BZlibCompressed = 0x80000006,

        /// <summary>
        /// The data is compressed using LZFSE data compression.
        /// </summary>
        LzfseCompressed = 0x80000007,

        /// <summary>
        /// The data is compressed uzing LZMA data compresion.
        /// </summary>
        LzmaCompressed = 0x80000008,

        /// <summary>
        /// The <see cref="CompressedRun"/> represents a comment, not actual data.
        /// </summary>
        Comment = 0x7FFFFFFE,

        /// <summary>
        /// The <see cref="CompressedRun"/> is the last entry. This run contains no
        /// actual data.
        /// </summary>
        Terminator = 0xFFFFFFFF,
    }
}