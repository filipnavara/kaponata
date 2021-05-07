// <copyright file="DeveloperDiskStore.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// The base class for repositories which store developer disk images.
    /// </summary>
    public abstract class DeveloperDiskStore
    {
        /// <summary>
        /// Adds a developer disk to the store.
        /// </summary>
        /// <param name="disk">
        /// The disk to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public abstract Task AddAsync(DeveloperDisk disk, CancellationToken cancellationToken);

        /// <summary>
        /// Lists all developer disks available in this store.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and, when completed, returns
        /// a list of all versions for which a developer disk is available.
        /// </returns>
        public abstract Task<List<Version>> ListAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a developer disk.
        /// </summary>
        /// <param name="version">
        /// The version for which to retrieve the developer disk.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation and, when completed,
        /// returns the requested dveloper disk.
        /// </returns>
        public abstract Task<DeveloperDisk> GetAsync(Version version, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a developer disk.
        /// </summary>
        /// <param name="version">
        /// The iOS version for which to delete the developer disk.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public abstract Task DeleteAsync(Version version, CancellationToken cancellationToken);
    }
}
