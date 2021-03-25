// <copyright file="IPropertyList.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;

namespace Kaponata.iOS.PropertyLists
{
    /// <summary>
    /// A common interface for all classes which represent data which can be serialized in a property list.
    /// </summary>
    public interface IPropertyList
    {
        /// <summary>
        /// Serializes this object as a <see cref="NSDictionary"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NSDictionary"/> which represents the current object.
        /// </returns>
        NSDictionary ToDictionary();
    }
}
