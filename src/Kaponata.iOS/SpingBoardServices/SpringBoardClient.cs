// <copyright file="SpringBoardClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.PropertyLists;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.SpingBoardServices
{
    /// <summary>
    /// The <see cref="SpringBoardClient"/> provides access to the springboard services running on the device.
    /// </summary>
    public class SpringBoardClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the springboard service on the device.
        /// </summary>
        public const string ServiceName = "com.apple.springboardservices";

        private readonly PropertyListProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringBoardClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the springboard services running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public SpringBoardClient(Stream stream, ILogger logger)
        {
            this.protocol = new PropertyListProtocol(stream, ownsStream: true, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringBoardClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the springboard services running on the device.
        /// </param>
        public SpringBoardClient(PropertyListProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Asynchronously gets the orientation of the device.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task<SpringBoardServicesInterfaceOrientation> GetInterfaceOrientationAsync(CancellationToken cancellationToken)
        {
            var request = new NSDictionary();
            request.Add("command", "getInterfaceOrientation");

            await this.protocol.WriteMessageAsync(request, cancellationToken).ConfigureAwait(false);

            var response = await this.protocol.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            return (SpringBoardServicesInterfaceOrientation)response.GetInt32("interfaceOrientation");
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return this.protocol.DisposeAsync();
        }
    }
}
