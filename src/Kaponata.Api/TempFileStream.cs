// <copyright file="TempFileStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.IO;

namespace Kaponata.Api
{
    /// <summary>
    /// A <see cref="Stream"/> backed by a temporary file.
    /// </summary>
    public class TempFileStream : FileStream
    {
        private readonly string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFileStream"/> class.
        /// </summary>
        public TempFileStream()
            : this(Path.GetTempFileName())
        {
        }

        private TempFileStream(string fileName)
            : base(fileName, FileMode.Create, FileAccess.ReadWrite)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Gets the name of the file which backs this <see cref="TempFileStream"/>.
        /// </summary>
        public string FileName => this.fileName;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                File.Delete(this.fileName);
            }
            catch
            {
                // Don't throw when disposing.
            }
        }
    }
}
