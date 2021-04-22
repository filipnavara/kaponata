// <copyright file="BlkxResource.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Collections.Generic;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Represents a <c>blkx</c> resource.
    /// </summary>
    public class BlkxResource : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlkxResource"/> class.
        /// </summary>
        /// <param name="parts">
        /// A dictionary which contains the metadata (property list data) of the blkx resource.
        /// </param>
        public BlkxResource(Dictionary<string, object> parts)
            : base("blkx", parts)
        {
            this.Block = EndianUtilities.ToStruct<CompressedBlock>(parts["Data"] as byte[], 0);
        }

        /// <summary>
        /// Gets the <see cref="CompressedBlock"/> which contains the data of this resource.
        /// </summary>
        public CompressedBlock Block { get; }
    }
}