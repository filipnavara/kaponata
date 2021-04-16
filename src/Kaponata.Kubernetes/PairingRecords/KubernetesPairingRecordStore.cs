// <copyright file="KubernetesPairingRecordStore.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Lockdown;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.PairingRecords
{
    /// <summary>
    /// Stores <see cref="PairingRecord"/> in a Kubernetes cluster as <see cref="V1Secret"/> objects.
    /// </summary>
    public class KubernetesPairingRecordStore : PairingRecordStore
    {
        private readonly KubernetesClient client;
        private readonly NamespacedKubernetesClient<V1Secret> secretClient;
        private readonly ILogger<KubernetesPairingRecordStore> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesPairingRecordStore"/> class.
        /// </summary>
        /// <param name="client">
        /// A <see cref="KubernetesClient"/> which can be used to connect to the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.</param>
        public KubernetesPairingRecordStore(KubernetesClient client, ILogger<KubernetesPairingRecordStore> logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.secretClient = this.client.GetClient<V1Secret>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesPairingRecordStore"/> class. Intended for mocking
        /// purposes only.
        /// </summary>
#nullable disable
        protected KubernetesPairingRecordStore()
        {
        }
#nullable restore

        /// <inheritdoc/>
        public override Task DeleteAsync(string udid, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            return this.secretClient.TryDeleteAsync(udid, TimeSpan.FromMinutes(1), cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<PairingRecord?> ReadAsync(string udid, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            var secret = await this.secretClient.TryReadAsync(udid, cancellationToken).ConfigureAwait(false);
            return secret.AsPairingRecord();
        }

        /// <inheritdoc/>
        public override Task WriteAsync(string udid, PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            if (udid == null)
            {
                throw new ArgumentNullException(nameof(udid));
            }

            if (pairingRecord == null)
            {
                throw new ArgumentNullException(nameof(pairingRecord));
            }

            var secret = pairingRecord.AsSecret();
            secret.Metadata.Name = udid;

            return this.secretClient.CreateAsync(secret, cancellationToken);
        }
    }
}
