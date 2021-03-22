// <copyright file="KubernetesDeveloperProfileTests.ProvisioningProfile.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.DeveloperProfiles;
using Kaponata.Kubernetes.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="KubernetesDeveloperProfile"/> class.
    /// </summary>
    public partial class KubernetesDeveloperProfileTests
    {
        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.GetProvisioningProfilesAsync(CancellationToken)"/>
        /// returns a profile list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfilesAsync_ReturnsProvisioningProfile_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.ListAsync(null, null, "kaponata.io/developerProfile=profile", null, default))
                .ReturnsAsync(
                new ItemList<V1Secret>()
                {
                    Items = new List<V1Secret>()
                    {
                        GetProvisioningProfileSecret(),
                    },
                });

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var profiles = await developerProfile.GetProvisioningProfilesAsync(default).ConfigureAwait(false);

            var profile = Assert.Single(profiles);
            Assert.Equal("CN=Apple iPhone OS Provisioning Profile Signing, O=Apple Inc., C=US", profile.SignerInfos[0].Certificate.Subject);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.GetProvisioningProfileAsync(Guid, CancellationToken)"/> returns
        /// the requested profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfileAsync_Found_ReturnsProvisioningProfile_Async()
        {
            var guid = Guid.NewGuid();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.TryReadAsync(guid.ToString(), "kaponata.io/developerProfile=profile", default))
                .ReturnsAsync(GetProvisioningProfileSecret());

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var profile = await developerProfile.GetProvisioningProfileAsync(guid, default).ConfigureAwait(false);
            Assert.Equal("CN=Apple iPhone OS Provisioning Profile Signing, O=Apple Inc., C=US", profile.SignerInfos[0].Certificate.Subject);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.GetProvisioningProfileAsync(Guid, CancellationToken)"/>
        /// returns <see langword="null"/> if the requested profile does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfileAsync_NotFound_ReturnsNull_Async()
        {
            var guid = Guid.NewGuid();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.TryReadAsync(guid.ToString(), "kaponata.io/developerProfile=profile", default))
                .ReturnsAsync((V1Secret)null);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            Assert.Null(await developerProfile.GetProvisioningProfileAsync(guid, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.AddProvisioningProfileAsync(SignedCms, CancellationToken)"/>
        /// validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddProvisioningProfileAsync_ValidatesArgument_Async()
        {
            var developerProfile = new KubernetesDeveloperProfile(Mock.Of<KubernetesClient>(), NullLogger<KubernetesDeveloperProfile>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => developerProfile.AddProvisioningProfileAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.AddProvisioningProfileAsync(SignedCms, CancellationToken)"/>
        /// adds a <see cref="V1Secret"/> which matches the profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddProvisioningProfileAsync_Works_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);
            secretClient
                .Setup(s => s.CreateAsync(It.IsAny<V1Secret>(), default))
                .Callback<V1Secret, CancellationToken>((secret, ct) =>
                {
                    Assert.Equal("98264c6b-5151-4349-8d0f-66691e48ae35", secret.Metadata.Name);
                    Assert.Equal(Annotations.ProvisioningProfile, secret.Metadata.Labels[Annotations.DeveloperProfileComponent]);

                    Assert.Equal("kaponata.io/signedData", secret.Type);
                    Assert.NotNull(secret.Data["signedData"]);
                })
                .ReturnsAsync(new V1Secret())
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var profile = new SignedCms();
            profile.Decode(File.ReadAllBytes("DeveloperProfiles/test.mobileprovision"));
            await developerProfile.AddProvisioningProfileAsync(profile, default).ConfigureAwait(false);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.DeleteProvisioningProfileAsync(Guid, CancellationToken)"/> throws an
        /// exception when no secret matching the certificat exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteProvisioningProfileAsync_NotFound_Throws_Async()
        {
            var guid = Guid.NewGuid();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            secretClient
                .Setup(c => c.TryReadAsync(guid.ToString(), "kaponata.io/developerProfile=profile", default))
                .ReturnsAsync((V1Secret)null)
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(() => developerProfile.DeleteProvisioningProfileAsync(guid, default)).ConfigureAwait(false);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.DeleteProvisioningProfileAsync(Guid, CancellationToken)"/> deletes
        /// the underlying secret.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteProvisioningProfileAsync_Works_Async()
        {
            var guid = Guid.NewGuid();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var secret = new V1Secret();
            secretClient
                .Setup(c => c.TryReadAsync(guid.ToString(), "kaponata.io/developerProfile=profile", default))
                .ReturnsAsync(secret)
                .Verifiable();

            secretClient
                .Setup(c => c.DeleteAsync(secret, TimeSpan.FromMinutes(1), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            await developerProfile.DeleteProvisioningProfileAsync(guid, default).ConfigureAwait(false);

            secretClient.Verify();
        }

        private static V1Secret GetProvisioningProfileSecret()
        {
            var secret = new V1Secret()
            {
                Data = new Dictionary<string, byte[]>(),
                Type = "kaponata.io/signedData",
            };

            secret.Data["signedData"] = File.ReadAllBytes("DeveloperProfiles/test.mobileprovision");

            return secret;
        }
    }
}
