﻿// <copyright file="LockdownException.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.iOS.Lockdown
{
    /// <summary>
    /// The exception which is thrown when an error occurs interacting with the Lockdown daemon.
    /// </summary>
    public class LockdownException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownException"/> class.
        /// </summary>
        public LockdownException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockdownException"/> class with a specified error
        /// message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public LockdownException(string message)
            : base(message)
        {
        }
    }
}