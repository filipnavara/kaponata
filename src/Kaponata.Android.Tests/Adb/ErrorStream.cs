// <copyright file="ErrorStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.IO;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// A class used to mock an error in the stream reading.
    /// </summary>
    public class ErrorStream : MemoryStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorStream"/> class.
        /// </summary>
        public ErrorStream()
            : base(100)
        {
        }

        /// <summary>
        /// Gets the invalid length.
        /// </summary>
        public override long Length => 300;
    }
}