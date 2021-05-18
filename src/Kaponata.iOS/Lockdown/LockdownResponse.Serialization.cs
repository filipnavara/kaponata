// <copyright file="LockdownResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Microsoft;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Serialization methods for <see cref="LockdownResponse{T}"/>.
    /// </content>
    public partial class LockdownResponse<T>
    {
        /// <summary>
        /// Reads a <see cref="LockdownResponse{T}"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="data">
        /// The message data.
        /// </param>
        public void FromDictionary(NSDictionary data)
        {
            Requires.NotNull(data, nameof(data));

            this.Domain = data.GetString(nameof(this.Domain));
            this.Key = data.GetString(nameof(this.Key));
            this.Request = data.GetString(nameof(this.Request));
            this.Result = data.GetString(nameof(this.Result));
            this.Type = data.GetString(nameof(this.Type));

            if (data.ContainsKey(nameof(this.Value)))
            {
                this.Value = (T)data.Get(nameof(this.Value)).ToObject();
            }

            this.Error = data.GetString(nameof(this.Error));
        }
    }
}
