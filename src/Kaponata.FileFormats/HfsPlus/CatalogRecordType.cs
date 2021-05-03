// <copyright file="CatalogRecordType.cs" company="Kenneth Bell, Quamotion bv">
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

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// describes the type of catalog data record.
    /// </summary>
    public enum CatalogRecordType : short
    {
        /// <summary>
        /// The type is unknown.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// This record is a folder record. You can use the <see cref="FolderRecord"/> type to interpret the data.
        /// </summary>
        FolderRecord = 0x0001,

        /// <summary>
        /// This record is a file record. You can use the <see cref="FileRecord"/> type to interpret the data.
        /// </summary>
        FileRecord = 0x0002,

        /// <summary>
        /// The record is a folder thread record. You can use the <see cref="FolderThreadRecord"/> type to interpret the data.
        /// </summary>
        FolderThreadRecord = 0x0003,

        /// <summary>
        /// The record is a file thread record. You can use the <see cref="FileThreadRecord"/> type to interpret the data.
        /// </summary>
        FileThreadRecord = 0x0004,
    }
}