// <copyright file="StartSessionResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents the resopnse to a <see cref="StartSessionRequest"/> request.
    /// </summary>
    public class StartSessionResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether SSL should be enabled.
        /// </summary>
        public bool EnableSessionSSL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the original request.
        /// </summary>
        public string Request
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a ID which uniquely identifies this session.
        /// </summary>
        public string SessionID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an error message which describes why the session could not be started.
        /// </summary>
        public string Error
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a <see cref="StartSessionResponse"/> from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dict">
        /// The dictionary from which to read the data.
        /// </param>
        /// <returns>
        /// A <see cref="StartSessionResponse"/> object which represents the deserialized data.
        /// </returns>
        public static StartSessionResponse Read(NSDictionary dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            return new StartSessionResponse()
            {
                EnableSessionSSL = dict.GetNullableBoolean(nameof(EnableSessionSSL)) ?? false,
                Error = dict.GetString(nameof(Error)),
                Request = dict.GetString(nameof(Request)),
                SessionID = dict.GetString(nameof(SessionID)),
            };
        }
    }
}
