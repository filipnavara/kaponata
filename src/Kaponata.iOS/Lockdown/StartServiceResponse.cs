// <copyright file="StartServiceResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents a response to the <see cref="StartServiceRequest"/> request.
    /// </summary>
    public partial class StartServiceResponse
    {
        /// <summary>
        /// Gets or sets the port at which the service is listening.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the service which was started.
        /// </summary>
        public string Service
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the request which was received by the device.
        /// </summary>
        public string Request
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an error message which describes the error which ocurred,
        /// if any.
        /// </summary>
        public string Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether SSL should be enabled or not.
        /// </summary>
        public bool EnableServiceSSL
        {
            get;
            set;
        }
    }
}
