// <copyright file="KindMetadata.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Contains the metadata of an Kubernetes object type, which is used to construct
    /// URLs for HTTP operations.
    /// </summary>
    public class KindMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KindMetadata"/> class.
        /// </summary>
        /// <param name="group">
        /// The Group used by Kubernetes to identify the object.
        /// </param>
        /// <param name="version">
        /// The Version used by Kubernetes to identify the object.
        /// </param>
        /// <param name="plural">
        /// The plural name of the object type, used by Kubernetes to identify the object.
        /// </param>
        public KindMetadata(string group, string version, string plural)
        {
            this.Group = group ?? throw new ArgumentNullException(nameof(group));
            this.Version = version ?? throw new ArgumentNullException(nameof(version));
            this.Plural = plural ?? throw new ArgumentNullException(nameof(plural));
        }

        /// <summary>
        /// Gets the Group used by Kubernetes to identify the object.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the Version used by Kubernetes to identify the object.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the plural name of the object type, used by Kubernetes to identify the object.
        /// </summary>
        public string Plural { get; }
    }
}
