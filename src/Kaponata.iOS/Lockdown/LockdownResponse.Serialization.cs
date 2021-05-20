// <copyright file="LockdownResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Microsoft;
using System;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Serialization methods for <see cref="LockdownResponse"/>.
    /// </content>
    public partial class LockdownResponse
    {
        /// <summary>
        /// Reads a <see cref="LockdownResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        public virtual void FromDictionary(NSDictionary data)
        {
            Requires.NotNull(data, nameof(data));

            this.Request = data.GetString(nameof(this.Request));
            this.Result = data.GetString(nameof(this.Result));

            var errorValue = data.GetString(nameof(this.Error));
            this.Error = errorValue == null ? null : Enum.Parse<LockdownError>(errorValue);
        }
    }
}
