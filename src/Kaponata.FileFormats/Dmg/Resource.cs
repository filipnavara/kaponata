// <copyright file="Resource.cs" company="Kenneth Bell, Quamotion bv">
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
//

#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// Represents an individual resource in the DMG file.
    /// </summary>
    /// <seealso href="http://newosxbook.com/DMG.html"/>
    internal abstract class Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        /// <param name="type">
        /// The resource data.
        /// </param>
        /// <param name="parts">
        /// Metadata (property list data) which describes the resource.
        /// </param>
        protected Resource(string type, Dictionary<string, object> parts)
        {
            this.Type = type;
            this.Name = parts["Name"] as string;

            string idStr = parts["ID"] as string;
            if (!string.IsNullOrEmpty(idStr))
            {
                int id;
                if (!int.TryParse(idStr, out id))
                {
                    throw new InvalidDataException("Invalid ID field");
                }

                this.Id = id;
            }

            string attrString = parts["Attributes"] as string;
            if (!string.IsNullOrEmpty(attrString))
            {
                NumberStyles style = NumberStyles.Integer;
                if (attrString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    style = NumberStyles.HexNumber;
                    attrString = attrString.Substring(2);
                }

                uint attributes;
                if (!uint.TryParse(attrString, style, CultureInfo.InvariantCulture, out attributes))
                {
                    throw new InvalidDataException("Invalid Attributes field");
                }

                this.Attributes = attributes;
            }
        }

        /// <summary>
        /// Gets or sets additional attributes of this resource.
        /// </summary>
        public uint Attributes { get; set; }

        /// <summary>
        /// Gets or sets a number which uniquely identifies this resource.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a user-friendly name of this resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type of resource.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Creates a <see cref="Resource"/> object based on the property list data.
        /// </summary>
        /// <param name="type">
        /// The resource type.
        /// </param>
        /// <param name="parts">
        /// The resource metadata.
        /// </param>
        /// <returns>
        /// A new <see cref="Resource"/> object.
        /// </returns>
        internal static Resource FromPlist(string type, Dictionary<string, object> parts)
        {
            switch (type)
            {
                case "blkx":
                    return new BlkxResource(parts);
                default:
                    return new GenericResource(type, parts);
            }
        }
    }
}