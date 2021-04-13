// <copyright file="ServiceScopeExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DependencyInjection
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceScope"/> class.
    /// </summary>
    public static class ServiceScopeExtensions
    {
        /// <summary>
        /// Asynchronously starts a service on the device.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the service to start.
        /// </typeparam>
        /// <param name="scope">
        /// A <see cref="IServiceScope"/> which represents a device scope.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, which returns a <typeparamref name="T"/>
        /// which represents a client for the service running on the device.
        /// </returns>
        public static Task<T> StartServiceAsync<T>(this IServiceScope scope, CancellationToken cancellationToken)
        {
            var factory = scope.ServiceProvider.GetRequiredService<ClientFactory<T>>();
            return factory.CreateAsync(cancellationToken);
        }
    }
}
