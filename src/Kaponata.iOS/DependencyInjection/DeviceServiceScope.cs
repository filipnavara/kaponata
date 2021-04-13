// <copyright file="DeviceServiceScope.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kaponata.iOS.DependencyInjection
{
    /// <summary>
    /// Represents a connection to a service running on a device.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the serivce running on the device.
    /// </typeparam>
    public struct DeviceServiceScope<T> : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceScope{T}"/> struct.
        /// </summary>
        /// <param name="scope">
        /// The service scope which represents the connection to the device.
        /// </param>
        /// <param name="device">
        /// The device on which the service is running.
        /// </param>
        /// <param name="service">
        /// A client for the service running on the device.
        /// </param>
        public DeviceServiceScope(IServiceScope scope, MuxerDevice device, T service)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.Device = device ?? throw new ArgumentNullException(nameof(device));
            this.Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets the service scope which represents the connection to the device.
        /// </summary>
        public IServiceScope Scope { get; }

        /// <summary>
        /// Gets the device on which the service is running.
        /// </summary>
        public MuxerDevice Device { get; }

        /// <summary>
        /// Gets a client for the service running on the device.
        /// </summary>
        public T Service { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Scope.Dispose();
        }
    }
}
