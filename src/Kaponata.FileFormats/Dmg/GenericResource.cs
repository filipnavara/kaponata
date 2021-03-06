// <copyright file="GenericResource.cs" company="Kenneth Bell, Quamotion bv">
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
using System.Collections.Generic;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// A generic resource in the DMG file.
    /// </summary>
    public class GenericResource : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericResource"/> class.
        /// </summary>
        /// <param name="type">
        /// The resource type.
        /// </param>
        /// <param name="parts">
        /// A dictionary (property list) which contains the resource metadata.
        /// </param>
        public GenericResource(string type, Dictionary<string, object> parts)
            : base(type, parts)
        {
            if (!parts.ContainsKey("Data") || !(parts["Data"] is byte[]))
            {
                throw new ArgumentOutOfRangeException(nameof(parts));
            }

            this.Data = (byte[])parts["Data"];
        }

        /// <summary>
        /// Gets additional data embedded in this resource.
        /// </summary>
        public byte[] Data { get; }
    }
}