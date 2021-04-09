// <copyright file="PairResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Serialization-related data for the <see cref="PairResponse"/> class.
    /// </content>
    public partial class PairResponse
    {
        /// <summary>
        /// Reads a <see cref="PairResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        /// <returns>
        /// A <see cref="PairResponse"/> object.
        /// </returns>
        public static PairResponse Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            PairResponse value = new PairResponse();
            value.Error = data.GetString(nameof(Error));
            value.EscrowBag = data.GetData(nameof(EscrowBag));

            return value;
        }
    }
}
