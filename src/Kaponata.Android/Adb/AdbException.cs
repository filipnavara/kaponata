// <copyright file="AdbException.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents errors that are reported from the <c>ADB</c> server.
    /// </summary>
    public class AdbException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class.
        /// </summary>
        public AdbException()
            : base("An unexpected ADB error occurred.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public AdbException(string message)
            : base(message)
        {
        }
    }
}
