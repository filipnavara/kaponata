// <copyright file="FileCompressionType.cs" company="Quamotion bv">
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
using System.Collections.Generic;
using System.Text;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// If a file is compressed, then it will have an extended attribute with name <c>com.apple.decmpfs</c>.
    /// Different compression types can be applied to a file, and this enum describes which compression type
    /// is being used.
    /// </summary>
    /// <seealso href="https://github.com/sleuthkit/sleuthkit/blob/dcf8136b04e77e8c28056e5b4639c97ffefa585e/tsk/fs/decmpfs.h"/>
    /// <seealso href="https://github.com/libyal/libfsapfs/blob/main/documentation/Apple%20File%20System%20(APFS).asciidoc#compression_method"/>
    /// <seealso href="https://github.com/RJVB/afsctool/issues/6#issuecomment-374620414"/>
    public enum FileCompressionType : uint
    {
        /// <summary>
        /// The compressed data is stored in the compression attribute, following the compression
        /// header. If the first byte of that data is 0xF, then the data is not compressed.
        /// Otherwise, the data is zlib-compressed.
        /// </summary>
        ZlibAttribute = 3,

        /// <summary>
        /// The compressed data is stored in the file's resource fork. The resource fork
        /// contains a single resource of type CMPF. The beginning of the resource is a
        /// table of offsets for successive zlib compression units within the resource.
        /// The compressed data is chunked in 64KB units.
        /// </summary>
        ZlibResource = 4,

        /// <summary>
        /// The file is dataless, that is, the uncompressed data contains 0-byte values.
        /// </summary>
        Dataless = 5,

        /// <summary>
        /// The compressed data is stored in the compression attribute, following the compression
        /// header. The data is LZVN-compressed.
        /// </summary>
        LzvnAttribute = 7,

        /// <summary>
        /// The compressed data is stored in the file's resource fork. The resource fork
        /// contains a single resource of type CMPF. The beginning of the resource is a
        /// table of offsets for successive LZVN compression units within the resource.
        /// The compressed data is chunked in 64KB units.
        /// </summary>
        LzvnResource = 8,

        /// <summary>
        /// The data is stored in the compression attribute, following the compression
        /// header. The data is not compressed.
        /// </summary>
        RawAttribute = 9,

        /// <summary>
        /// The compressed data is stored in the file's resource fork. The resource fork
        /// contains a single resource of type CMPF. The beginning of the resource is a
        /// table of offsets for successive uncompressed units within the resource.
        /// The data is chunked in 64KB units.
        /// </summary>
        RawResource = 10,

        /// <summary>
        /// The compressed data is stored in the compression attribute, following the compression
        /// header. The data is LZFSE-compressed.
        /// </summary>
        LzfseAttribute = 11,

        /// <summary>
        /// The compressed data is stored in the file's resource fork. The resource fork
        /// contains a single resource of type CMPF. The beginning of the resource is a
        /// table of offsets for successive LZFSE compression units within the resource.
        /// The compressed data is chunked in 64KB units.
        /// </summary>
        LzfseResource = 12,
    }
}
