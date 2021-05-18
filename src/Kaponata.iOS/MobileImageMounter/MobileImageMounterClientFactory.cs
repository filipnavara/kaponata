// <copyright file="MobileImageMounterClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.MobileImageMounter
{
    /// <summary>
    /// Connects to the Mobile Image Mounter service running on an iOS device, and creates a <see cref="MobileImageMounterClient"/> objects.
    /// </summary>
    public class MobileImageMounterClientFactory : ServiceClientFactory<MobileImageMounterClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MobileImageMounterClientFactory"/> class.
        /// </summary>
        /// <param name="muxer">
        /// The <see cref="MuxerClient"/> which represents the connection to the iOS USB Multiplexor.
        /// </param>
        /// <param name="context">
        /// The <see cref="DeviceContext"/> which contains information about the device with which
        /// we are interacting.
        /// </param>
        /// <param name="lockdownClientFactory">
        /// A <see cref="LockdownClientFactory"/> which can create a connection to lockdown.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        public MobileImageMounterClientFactory(MuxerClient muxer, DeviceContext context, LockdownClientFactory lockdownClientFactory, ILogger<MobileImageMounterClient> logger)
            : base(muxer, context, lockdownClientFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override async Task<MobileImageMounterClient> CreateAsync(string serviceName, CancellationToken cancellationToken)
        {
            var serviceStream = await this.StartServiceAsync(serviceName, cancellationToken);
            return new MobileImageMounterClient(serviceStream, this.Logger);
        }

        /// <inheritdoc/>
        public override Task<MobileImageMounterClient> CreateAsync(CancellationToken cancellationToken) => this.CreateAsync(MobileImageMounterClient.ServiceName, cancellationToken);
    }
}
