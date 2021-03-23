// <copyright file="KubernetesDeveloperProfile.X509Certificate2.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.DeveloperProfiles
{
    /// <summary>
    /// Emulates an iOS developer profile and stores the developer certificates (identities) and provisioning profiles
    /// as secrets in the Kubernetes cluster.
    /// </summary>
    public partial class KubernetesDeveloperProfile
    {
        private static readonly string DeveloperIdentityLabelSelector = LabelSelector.Create(
                    new Dictionary<string, string>()
                    {
                        { Annotations.DeveloperProfileComponent, Annotations.DeveloperIdentity },
                    });

        /// <summary>
        /// Asynchronously lists all iOS development certificates in this cluster.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IList<X509Certificate2>> GetCertificatesAsync(CancellationToken cancellationToken)
        {
            var response = await this.secretClient.ListAsync(
                labelSelector: DeveloperIdentityLabelSelector,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Items.Select(s => s.AsX509Certificate2()).ToList();
        }

        /// <summary>
        /// Asynchronously retrieves an individual certificate from the cluster.
        /// </summary>
        /// <param name="thumbprint">
        /// The thumbprint of the certificate.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<X509Certificate2?> GetCertificateAsync(string thumbprint, CancellationToken cancellationToken)
        {
            var secret = await this.secretClient.TryReadAsync(thumbprint, DeveloperIdentityLabelSelector, cancellationToken).ConfigureAwait(false);

            if (secret == null)
            {
                return null;
            }

            return secret.AsX509Certificate2();
        }

        /// <summary>
        /// Asynchronously adds a certificate to the cluster.
        /// </summary>
        /// <param name="certificate">
        /// The certificate to add to the cluster.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddCertificateAsync(X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var secret = certificate.AsSecret();
            secret.Metadata.Labels[Annotations.DeveloperProfileComponent] = Annotations.DeveloperIdentity;

            await this.secretClient.CreateAsync(secret, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously deletes a certificate from the cluster.
        /// </summary>
        /// <param name="thumbprint">
        /// The thumbprint of the certificate to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteCertificateAsync(string thumbprint, CancellationToken cancellationToken)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException(nameof(thumbprint));
            }

            var secret = await this.secretClient.TryReadAsync(thumbprint, DeveloperIdentityLabelSelector, cancellationToken).ConfigureAwait(false);

            if (secret == null)
            {
                throw new InvalidOperationException();
            }

            await this.secretClient.DeleteAsync(secret, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);
        }
    }
}
