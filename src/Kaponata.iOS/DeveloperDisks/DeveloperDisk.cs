// <copyright file="DeveloperDisk.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft;
using Nerdbank.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// Represents an individual developer disk.
    /// </summary>
    public class DeveloperDisk : IDisposableObservable
    {
        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the version information embedded in the developer disk.
        /// </summary>
        public SystemVersion Version { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Stream"/> which provides access to the developer disk image.
        /// </summary>
        public Stream Image { get; set; }

        /// <summary>
        /// Gets or sets the signature of the developer disk.
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the developer disk image was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Image?.Dispose();
            this.IsDisposed = true;
        }
    }
}
