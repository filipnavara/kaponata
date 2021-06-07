// <copyright file="LicenseStore.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kaponata.Kubernetes.Licensing
{
    /// <summary>
    /// Stores Kaponata license files.
    /// </summary>
    public class LicenseStore
    {
        private const string ConfigMapName = "kaponata-license";
        private readonly KubernetesClient kubernetesClient;
        private readonly ILogger<LicenseStore> logger;
        private readonly NamespacedKubernetesClient<V1ConfigMap> configClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseStore"/> class.
        /// </summary>
        /// <param name="kubernetesClient">
        /// A <see cref="kubernetesClient"/> which provides access to the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public LicenseStore(KubernetesClient kubernetesClient, ILogger<LicenseStore> logger)
        {
            this.kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configClient = this.kubernetesClient.GetClient<V1ConfigMap>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseStore"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking purposes only.
        /// </remarks>
#nullable disable
        protected LicenseStore()
        {
        }
#nullable restore

        /// <summary>
        /// Asynchronously adds a new license to the cluster.
        /// </summary>
        /// <param name="license">
        /// A <see cref="XDocument"/> which represents the license to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task AddLicenseAsync(XDocument license, CancellationToken cancellationToken)
        {
            if (license == null)
            {
                throw new ArgumentNullException(nameof(license));
            }

            var data = new Dictionary<string, string>();
            data.Add("license", license.ToString());

            var current = await this.configClient.TryReadAsync(ConfigMapName, cancellationToken).ConfigureAwait(false);

            if (current == null)
            {
                await this.configClient.CreateAsync(
                    new V1ConfigMap()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = ConfigMapName,
                        },
                        Data = data,
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var patch = new JsonPatchDocument<V1ConfigMap>();
                patch.Replace(p => p.Data, data);

                await this.configClient.PatchAsync(
                    current,
                    patch,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the current license.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, which returns the license file
        /// (or <see langword="null"/> when none is available) when completed.</returns>
        public virtual async Task<XDocument?> GetLicenseAsync(CancellationToken cancellationToken)
        {
            var license = await this.configClient.TryReadAsync(ConfigMapName, cancellationToken).ConfigureAwait(false);

            if (license == null)
            {
                return null;
            }

            return XDocument.Parse(license.Data["license"]);
        }
    }
}
