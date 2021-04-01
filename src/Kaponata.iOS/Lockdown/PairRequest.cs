// <copyright file="PairRequest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents a request related to a pair record.
    /// </summary>
    public class PairRequest : LockdownMessage
    {
        /// <summary>
        /// Gets or sets the pair record.
        /// </summary>
        public PairingRecord PairRecord { get; set; }

        /// <summary>
        /// Gets or sets an optional <see cref="PairingOptions"/> value.
        /// </summary>
        public PairingOptions PairingOptions { get; set; }

        /// <inheritdoc/>
        public override NSDictionary ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add(nameof(this.PairRecord), this.PairRecord.ToPropertyList(includePrivateKeys: false));

            if (this.PairingOptions != null)
            {
                dictionary.Add(
                    nameof(this.PairingOptions), this.PairingOptions.ToPropertyList());
            }

            return dictionary;
        }
    }
}
