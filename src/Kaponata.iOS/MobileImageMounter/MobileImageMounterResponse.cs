// <copyright file="MobileImageMounterResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.PropertyLists;
using Microsoft;

namespace Kaponata.iOS.MobileImageMounter
{
    /// <summary>
    /// Respresents the response of a image mounter request.
    /// </summary>
    public class MobileImageMounterResponse : IPropertyListDeserializable
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response error.
        /// </summary>
        public string Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the detailed error.
        /// </summary>
        public string DetailedError
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public virtual void FromDictionary(NSDictionary dictionary)
        {
            Requires.NotNull(dictionary, nameof(dictionary));

            this.Status = dictionary.GetString(nameof(this.Status));
            this.Error = dictionary.GetString(nameof(this.Error));
            this.DetailedError = dictionary.GetString(nameof(this.DetailedError));
        }
    }
}
