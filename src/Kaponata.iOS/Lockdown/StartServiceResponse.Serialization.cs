// <copyright file="StartServiceResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Methods for deserializing the <see cref="StartServiceResponse"/>.
    /// </content>
    public partial class StartServiceResponse
    {
        /// <summary>
        /// Deserializes a <see cref="StartServiceResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// A dictionary which represents the response.
        /// </param>
        /// <returns>
        /// A <see cref="StartServiceResponse"/> object which represents the response.
        /// </returns>
        public static StartServiceResponse Read(NSDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            return new StartServiceResponse()
            {
                EnableServiceSSL = dict.GetNullableBoolean(nameof(EnableServiceSSL)) ?? false,
                Error = dict.GetString(nameof(Error)),
                Port = dict.GetNullableInt32(nameof(Port)) ?? 0,
                Request = dict.GetString(nameof(Request)),
                Service = dict.GetString(nameof(Service)),
            };
        }
    }
}
