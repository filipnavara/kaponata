// <copyright file="ResourceFork.cs" company="Kenneth Bell, Quamotion bv">
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

using System;
using System.Collections.Generic;

namespace DiscUtils.Dmg
{
    /// <summary>
    /// The main resource fork of this DMG file. Embedded as a property list in the
    /// trailer of the DMG file.
    /// </summary>
    public class ResourceFork
    {
        private readonly List<Resource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFork"/> class.
        /// </summary>
        /// <param name="resources">
        /// The individual resources in the resource fork.
        /// </param>
        public ResourceFork(List<Resource> resources)
        {
            this.resources = resources;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceFork"/> object based on the metadata.
        /// </summary>
        /// <param name="plist">
        /// The property list data which describes the <see cref="ResourceFork"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ResourceFork"/> object.
        /// </returns>
        public static ResourceFork FromPlist(Dictionary<string, object> plist)
        {
            if (plist == null)
            {
                throw new ArgumentNullException(nameof(plist));
            }

            object typesObject;
            if (!plist.TryGetValue("resource-fork", out typesObject))
            {
                throw new ArgumentException("plist doesn't contain resource fork");
            }

            Dictionary<string, object> types = typesObject as Dictionary<string, object>;

            List<Resource> resources = new List<Resource>();

            foreach (string type in types.Keys)
            {
                var typeResources = types[type] as IEnumerable<object>;
                foreach (object typeResource in typeResources)
                {
                    resources.Add(Resource.FromPlist(type, typeResource as Dictionary<string, object>));
                }
            }

            return new ResourceFork(resources);
        }

        /// <summary>
        /// Lists all resources of a given type which are embedded in this DMG file.
        /// </summary>
        /// <param name="type">
        /// The the type of resource to retrieve.
        /// </param>
        /// <returns>
        /// A list of all resources of the requested type.
        /// </returns>
        public IList<Resource> GetAllResources(string type)
        {
            List<Resource> results = new List<Resource>();

            foreach (Resource res in this.resources)
            {
                if (res.Type == type)
                {
                    results.Add(res);
                }
            }

            return results;
        }
    }
}