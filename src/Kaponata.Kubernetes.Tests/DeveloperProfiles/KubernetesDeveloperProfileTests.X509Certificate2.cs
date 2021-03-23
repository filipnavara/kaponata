// <copyright file="KubernetesDeveloperProfileTests.X509Certificate2.cs" company="Quamotion bv">
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
using System.Security.Cryptography.X509Certificates;
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
        /// <see cref="KubernetesDeveloperProfile.GetCertificatesAsync(CancellationToken)"/>
        /// returns a certificate list.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificatesAsync_ReturnsCertificate_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.ListAsync(null, null, "kaponata.io/developerProfile=identity", null, default))
                .ReturnsAsync(
                new ItemList<V1Secret>()
                {
                    Items = new List<V1Secret>()
                    {
                        GetSecret(),
                    },
                });

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var certificates = await developerProfile.GetCertificatesAsync(default).ConfigureAwait(false);

            var certificate = Assert.Single(certificates);
            Assert.Equal("CN=Test", certificate.Subject);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.GetCertificateAsync(string, CancellationToken)"/> returns
        /// the requested certificate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificateAsync_Found_ReturnsCertificate_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.TryReadAsync("1234", "kaponata.io/developerProfile=identity", default))
                .ReturnsAsync(GetSecret());

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var certificate = await developerProfile.GetCertificateAsync("1234", default).ConfigureAwait(false);
            Assert.Equal("CN=Test", certificate.Subject);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.GetCertificateAsync(string, CancellationToken)"/>
        /// returns <see langword="null"/> if the requested certificate does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificateAsync_NotFound_ReturnsNull_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            secretClient
                .Setup(s => s.TryReadAsync("1234", "kaponata.io/developerProfile=identity", default))
                .ReturnsAsync((V1Secret)null);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            Assert.Null(await developerProfile.GetCertificateAsync("1234", default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.AddCertificateAsync(X509Certificate2, CancellationToken)"/>
        /// validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddCertificateAsync_ValidatesArgument_Async()
        {
            var developerProfile = new KubernetesDeveloperProfile(Mock.Of<KubernetesClient>(), NullLogger<KubernetesDeveloperProfile>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => developerProfile.AddCertificateAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.AddCertificateAsync(X509Certificate2, CancellationToken)"/>
        /// adds a <see cref="V1Secret"/> which matches the certificate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddCertificateAsync_Works_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);
            secretClient
                .Setup(s => s.CreateAsync(It.IsAny<V1Secret>(), default))
                .Callback<V1Secret, CancellationToken>((secret, ct) =>
                {
                    Assert.Equal("c973377d7991bebbe3d85dffe838a61f5283d621", secret.Metadata.Name);
                    Assert.Equal(Annotations.DeveloperIdentity, secret.Metadata.Labels[Annotations.DeveloperProfileComponent]);

                    Assert.Equal("kubernetes.io/tls", secret.Type);
                    Assert.NotNull(secret.Data["tls.key"]);
                    Assert.NotNull(secret.Data["tls.crt"]);
                })
                .ReturnsAsync(new V1Secret())
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            var certificate = X509Certificate2.CreateFromPemFile("DeveloperProfiles/tls.crt", "DeveloperProfiles/tls.key");
            await developerProfile.AddCertificateAsync(certificate, default).ConfigureAwait(false);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.DeleteCertificateAsync(string, CancellationToken)"/> validates its
        /// arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCertificateAsync_ValidatesArgument_Async()
        {
            var developerProfile = new KubernetesDeveloperProfile(Mock.Of<KubernetesClient>(), NullLogger<KubernetesDeveloperProfile>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => developerProfile.DeleteCertificateAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.DeleteCertificateAsync(string, CancellationToken)"/> throws an
        /// exception when no secret matching the certificat exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCertificateAsync_NotFound_Throws_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            secretClient
                .Setup(c => c.TryReadAsync("abc", "kaponata.io/developerProfile=identity", default))
                .ReturnsAsync((V1Secret)null)
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(() => developerProfile.DeleteCertificateAsync("abc", default)).ConfigureAwait(false);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesDeveloperProfile.DeleteCertificateAsync(string, CancellationToken)"/> deletes
        /// the underlying secret.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCertificateAsync_Works_Async()
        {
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);

            kubernetes.Setup(k => k.GetClient<V1Secret>()).Returns(secretClient.Object);

            var secret = new V1Secret();
            secretClient
                .Setup(c => c.TryReadAsync("c973377d7991bebbe3d85dffe838a61f5283d621", "kaponata.io/developerProfile=identity", default))
                .ReturnsAsync(secret)
                .Verifiable();

            secretClient
                .Setup(c => c.DeleteAsync(secret, TimeSpan.FromMinutes(1), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var developerProfile = new KubernetesDeveloperProfile(kubernetes.Object, NullLogger<KubernetesDeveloperProfile>.Instance);

            await developerProfile.DeleteCertificateAsync("c973377d7991bebbe3d85dffe838a61f5283d621", default).ConfigureAwait(false);

            secretClient.Verify();
        }

        private static V1Secret GetSecret()
        {
            var secret = new V1Secret()
            {
                Data = new Dictionary<string, byte[]>(),
                Type = "kubernetes.io/tls",
            };

            secret.Data["tls.crt"] = File.ReadAllBytes("DeveloperProfiles/tls.crt");
            secret.Data["tls.key"] = File.ReadAllBytes("DeveloperProfiles/tls.key");

            return secret;
        }
    }
}
