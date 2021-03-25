﻿// <copyright file="LockdownResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// Represents a response sent from lockdown running on the device to the PC. This class is generic, and not
    /// all properties may have values.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    public partial class LockdownResponse<T>
    {
        /// <summary>
        /// Gets or sets the name of the domain of the value which is being returned.
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the value which being returned.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the request to which a value is being sent.
        /// </summary>
        public string Request
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a string which represents the result (success, failure,...) of the operation.
        /// </summary>
        public string Result
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        public T Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Error
        {
            get;
            set;
        }
    }
}
