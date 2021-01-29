// <copyright file="SyncService.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Provides a client interface for the <c>ADB</c> sync service.
    /// </summary>
    public class SyncService : IAsyncDisposable
    {
        private readonly ILogger<SyncService> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly AdbProtocol adbProtocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A <see cref="ILoggerFactory"/> which can be used to initialise new loggers.
        /// </param>
        /// <param name="protocol">
        /// A <see cref="AdbProtocol"/> on  which this <see cref="SyncService"/> is operating.
        /// </param>
        public SyncService(ILogger<SyncService> logger, ILoggerFactory loggerFactory, AdbProtocol protocol)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.adbProtocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SyncService"/> is connected.
        /// </summary>
        public bool Connected { get; private set; } = false;

        /// <summary>
        /// Connects to the sync service.
        /// </summary>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task ConnectAsync(DeviceData device, CancellationToken cancellationToken)
        {
            await this.adbProtocol.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);
            await this.adbProtocol.StartSyncSessionAsync(cancellationToken).ConfigureAwait(false);
            this.Connected = true;
        }

        /// <summary>
        /// Ensures this <see cref="SyncService"/> is connnected.
        /// </summary>
        public void EnsureConnected()
        {
            if (!this.Connected)
            {
                throw new InvalidOperationException("Not connected to the sync service.");
            }
        }

        /// <summary>
        /// Lists the files from the remote directory.
        /// </summary>
        /// <param name="remotePath">
        /// The remote path.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task<IList<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken)
        {
            List<FileStatistics> entries = new List<FileStatistics>();

            await this.adbProtocol.WriteSyncCommandAsync(SyncCommandType.LIST, remotePath, cancellationToken).ConfigureAwait(false);

            while (true)
            {
                var response = await this.adbProtocol.ReadSyncCommandTypeAsync(cancellationToken).ConfigureAwait(false);

                if (response == SyncCommandType.DENT)
                {
                    var entry = await this.adbProtocol.ReadFileStatisticsAsync(cancellationToken).ConfigureAwait(false);
                    entries.Add(entry);
                }
                else if (response == SyncCommandType.DONE)
                {
                    break;
                }
                else
                {
                    throw new AdbException($"The server returned an invalid sync response: {response}");
                }
            }

            return entries;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.adbProtocol.DisposeAsync().ConfigureAwait(false);
            this.Connected = false;
        }
    }
}
