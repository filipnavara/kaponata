// <copyright file="PairingOptions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents optional options which can be sent with a <see cref="PairRequest"/> message.
    /// </summary>
    public class PairingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether extended pairing errors messages should be
        /// returned by the device.
        /// </summary>
        public bool ExtendedPairingErrors { get; set; }

        /// <summary>
        /// Converts this <see cref="PairingOptions"/> object to a  <see cref="NSDictionary"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NSDictionary"/> which represents this <see cref="PairingOptions"/> object.
        /// </returns>
        public NSDictionary ToPropertyList()
        {
            NSDictionary dict = new NSDictionary();
            dict.Add(nameof(this.ExtendedPairingErrors), new NSNumber(this.ExtendedPairingErrors));
            return dict;
        }
    }
}
