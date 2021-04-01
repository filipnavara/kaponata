// <copyright file="LockdownClient.StartService.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Methods to start services on iOS devices.
    /// </content>
    public partial class LockdownClient
    {
        /// <summary>
        /// Starts a service on the device.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service to start.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous request.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous response. The return value is the port
        /// at which the service is listening.
        /// </returns>
        public virtual async Task<ServiceDescriptor> StartServiceAsync(string serviceName, CancellationToken cancellationToken)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            await this.protocol.WriteMessageAsync(
                new StartServiceRequest()
                {
                    Label = this.Label,
                    Request = "StartService",
                    Service = serviceName,
                },
                cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            var message = StartServiceResponse.Read(response);

            if (message.Error != null)
            {
                throw new LockdownException(message.Error);
            }

            return new ServiceDescriptor()
            {
                Port = unchecked((ushort)message.Port),
                EnableServiceSSL = message.EnableServiceSSL,
                ServiceName = serviceName,
            };
        }
    }
}
