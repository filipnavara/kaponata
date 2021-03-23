// <copyright file="KubernetesDeveloperProfile.ProvisioningProfile.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
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
        private static readonly string ProvisioningProfileLabelSelector = LabelSelector.Create(
                    new Dictionary<string, string>()
                    {
                        { Annotations.DeveloperProfileComponent, Annotations.ProvisioningProfile },
                    });

        /// <summary>
        /// Asynchronously lists all provisioning profiles which are stored in this cluster.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IList<SignedCms>> GetProvisioningProfilesAsync(CancellationToken cancellationToken)
        {
            var response = await this.secretClient.ListAsync(
                labelSelector: ProvisioningProfileLabelSelector,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Items.Select(s => s.AsSignedCms()).ToList();
        }

        /// <summary>
        /// Asynchronously retrieves an individual provisioning profile from the cluster.
        /// </summary>
        /// <param name="uuid">
        /// The UUID of the provisioning profile to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<SignedCms?> GetProvisioningProfileAsync(Guid uuid, CancellationToken cancellationToken)
        {
            var secret = await this.secretClient.TryReadAsync(uuid.ToString(), ProvisioningProfileLabelSelector, cancellationToken).ConfigureAwait(false);

            if (secret == null)
            {
                return null;
            }

            return secret.AsSignedCms();
        }

        /// <summary>
        /// Asynchronously adds a provisioning profile to the cluster.
        /// </summary>
        /// <param name="profile">
        /// A <see cref="SignedCms"/> object which represents the provisioning profile to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddProvisioningProfileAsync(SignedCms profile, CancellationToken cancellationToken)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var secret = profile.AsSecret();
            var signedProfile = ProvisioningProfile.Read(profile);

            secret.Metadata.Name = signedProfile.Uuid.ToString();
            secret.Metadata.Labels[Annotations.DeveloperProfileComponent] = Annotations.ProvisioningProfile;

            await this.secretClient.CreateAsync(secret, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously deletes a provisioning profile from the cluster.
        /// </summary>
        /// <param name="uuid">
        /// The UUID of the provisioning profile to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteProvisioningProfileAsync(Guid uuid, CancellationToken cancellationToken)
        {
            var secret = await this.secretClient.TryReadAsync(uuid.ToString(), ProvisioningProfileLabelSelector, cancellationToken).ConfigureAwait(false);

            if (secret == null)
            {
                throw new InvalidOperationException();
            }

            await this.secretClient.DeleteAsync(secret, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);
        }
    }
}
