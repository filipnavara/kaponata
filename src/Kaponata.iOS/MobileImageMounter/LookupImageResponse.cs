// <copyright file="LookupImageResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System.Collections.Generic;

namespace Kaponata.iOS.MobileImageMounter
{
    /// <summary>
    /// Respresents the response of a LookupImage request.
    /// </summary>
    public class LookupImageResponse : MobileImageMounterResponse
    {
        /// <summary>
        /// Gets or sets the signature object as retrieved from the device.
        /// </summary>
        public IList<byte[]> ImageSignature { get; set; }

        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary dictionary)
        {
            base.FromDictionary(dictionary);

            this.ImageSignature = dictionary.GetDataArray(nameof(this.ImageSignature));
        }
    }
}
