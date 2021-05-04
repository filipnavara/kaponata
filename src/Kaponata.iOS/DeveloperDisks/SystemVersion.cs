// <copyright file="SystemVersion.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.PropertyLists;
using System;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// Represents the version of an operating system or a SDK.
    /// </summary>
    public class SystemVersion : IPropertyListDeserializable
    {
        /// <summary>
        /// Gets or sets a <see cref="Guid"/> which uniquely identifies the product build.
        /// </summary>
        public Guid BuildID { get; set; }

        /// <summary>
        /// Gets or sets the product build version.
        /// </summary>
        public AppleVersion ProductBuildVersion { get; set; }

        /// <summary>
        /// Gets or sets the copyright owner.
        /// </summary>
        public string ProductCopyright { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the version of the product.
        /// </summary>
        public Version ProductVersion { get; set; }

        /// <inheritdoc/>
        public void FromDictionary(NSDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.ContainsKey(nameof(this.BuildID)))
            {
                this.BuildID = new Guid(dictionary.GetString(nameof(this.BuildID)));
            }

            this.ProductBuildVersion = AppleVersion.Parse(dictionary.GetString(nameof(this.ProductBuildVersion)));
            this.ProductCopyright = dictionary.GetString(nameof(this.ProductCopyright));
            this.ProductName = dictionary.GetString(nameof(this.ProductName));
            this.ProductVersion = new Version(dictionary.GetString(nameof(this.ProductVersion)));
        }
    }
}
