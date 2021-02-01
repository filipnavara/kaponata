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
        private readonly bool canRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorStream"/> class.
        /// </summary>
        /// <param name="canRead">
        /// Indicates whether the stream is readable.
        /// </param>
        public ErrorStream(bool canRead = true)
            : base(100)
        {
            this.canRead = canRead;
        }

        /// <inheritdoc/>
        public override bool CanRead => this.canRead;

        /// <summary>
        /// Gets the invalid length.
        /// </summary>
        public override long Length => 300;
    }
}
