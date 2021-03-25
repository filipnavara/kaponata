// <copyright file="LockdownResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;

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
        /// <returns>
        /// A <see cref="LockdownResponse{T}"/> object.
        /// </returns>
        public static LockdownResponse<T> Read(NSDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            LockdownResponse<T> value = new LockdownResponse<T>();

            value.Domain = data.GetString(nameof(Domain));
            value.Key = data.GetString(nameof(Key));
            value.Request = data.GetString(nameof(Request));
            value.Result = data.GetString(nameof(Result));
            value.Type = data.GetString(nameof(Type));

            if (data.ContainsKey(nameof(Value)))
            {
                value.Value = (T)data.Get(nameof(Value)).ToObject();
            }

            value.Error = data.GetString(nameof(Error));

            return value;
        }
    }
}
