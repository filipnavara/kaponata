// <copyright file="KubernetesDeveloperProfile.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Kaponata.Kubernetes.DeveloperProfiles
{
    /// <summary>
    /// Emulates an iOS developer profile and stores the developer certificates (identities) and provisioning profiles
    /// as secrets in the Kubernetes cluster.
    /// </summary>
    public partial class KubernetesDeveloperProfile
    {
        private readonly KubernetesClient kubernetes;
        private readonly NamespacedKubernetesClient<V1Secret> secretClient;
        private readonly ILogger<KubernetesDeveloperProfile> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesDeveloperProfile"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A <see cref="KubernetesClient"/> which represents the connection to the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public KubernetesDeveloperProfile(KubernetesClient kubernetes, ILogger<KubernetesDeveloperProfile> logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.secretClient = kubernetes.GetClient<V1Secret>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesDeveloperProfile"/> class.
        /// Intended for unit testing/mocking purposes only.
        /// </summary>
#nullable disable
        protected KubernetesDeveloperProfile()
        {
        }
#nullable restore
    }
}
