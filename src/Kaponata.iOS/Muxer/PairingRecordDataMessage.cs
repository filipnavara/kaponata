// <copyright file="PairingRecordDataMessage.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.Muxer
{
    /// <summary>
    /// Represents the response to the <see cref="ReadPairingRecordMessage"/> request.
    /// </summary>
    public partial class PairingRecordDataMessage : ResultMessage
    {
        /// <summary>
        /// Gets or sets the pairing record data, as a binary blob.
        /// </summary>
        public byte[] PairRecordData
        {
            get;
            set;
        }
    }
}
