// <copyright file="PairResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents the device's response to a <see cref="PairRequest"/>.
    /// </summary>
    public partial class PairResponse
    {
        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the escrow bag, if any.
        /// </summary>
        public byte[] EscrowBag { get; set; }
    }
}
