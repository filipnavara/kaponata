// <copyright file="PairResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Serialization-related data for the <see cref="PairResponse"/> class.
    /// </content>
    public partial class PairResponse
    {
        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            this.EscrowBag = data.GetData(nameof(this.EscrowBag));
        }
    }
}
