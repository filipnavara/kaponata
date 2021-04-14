// <copyright file="KubernetesPairingRecordStoreTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Lockdown;
using Kaponata.Kubernetes.PairingRecords;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.PairingRecords
{
    /// <summary>
    /// Tests the <see cref="KubernetesPairingRecordStore"/> class.
    /// </summary>
    public class KubernetesPairingRecordStoreTests
    {
        /// <summary>
        /// The <see cref="KubernetesPairingRecordStore"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new KubernetesPairingRecordStore(null, NullLogger<KubernetesPairingRecordStore>.Instance));
            Assert.Throws<ArgumentNullException>(() => new KubernetesPairingRecordStore(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.DeleteAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ValidatesArguments_Async()
        {
            var store = new KubernetesPairingRecordStore(Mock.Of<KubernetesClient>(), NullLogger<KubernetesPairingRecordStore>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => store.DeleteAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.ReadAsync(string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadAsync_ValidatesArguments_Async()
        {
            var store = new KubernetesPairingRecordStore(Mock.Of<KubernetesClient>(), NullLogger<KubernetesPairingRecordStore>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => store.ReadAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.WriteAsync(string, PairingRecord, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteAsync_ValidatesArguments_Async()
        {
            var store = new KubernetesPairingRecordStore(Mock.Of<KubernetesClient>(), NullLogger<KubernetesPairingRecordStore>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => store.WriteAsync(null, new PairingRecord(), default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.WriteAsync("abc", null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.DeleteAsync(string, CancellationToken)"/> works as expected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_Works_Async()
        {
            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1Secret>()).Returns(secretClient.Object);

            secretClient
                .Setup(c => c.TryDeleteAsync("abc", TimeSpan.FromMinutes(1), default))
                .Returns(Task.FromResult((V1Secret)null))
                .Verifiable();

            var store = new KubernetesPairingRecordStore(client.Object, NullLogger<KubernetesPairingRecordStore>.Instance);

            await store.DeleteAsync("abc", default);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.ReadAsync(string, CancellationToken)"/> works as expected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadAsync_Works_Async()
        {
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var pairingRecord = new PairingRecordGenerator().Generate(key, "abc");
            var secret = pairingRecord.AsSecret();

            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1Secret>()).Returns(secretClient.Object);

            secretClient
                .Setup(c => c.TryReadAsync("abc", default))
                .Returns(Task.FromResult(secret))
                .Verifiable();

            var store = new KubernetesPairingRecordStore(client.Object, NullLogger<KubernetesPairingRecordStore>.Instance);

            var value = await store.ReadAsync("abc", default);

            // Basic assertions
            Assert.NotNull(value.DeviceCertificate);
            Assert.NotNull(value.HostCertificate);
            Assert.NotNull(value.RootCertificate);

            secretClient.Verify();
        }

        /// <summary>
        /// <see cref="KubernetesPairingRecordStore.WriteAsync(string, PairingRecord, CancellationToken)"/> works as expected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteAsync_Works_Async()
        {
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var pairingRecord = new PairingRecordGenerator().Generate(key, "abc");
            var secret = pairingRecord.AsSecret();

            var client = new Mock<KubernetesClient>(MockBehavior.Strict);
            var secretClient = new Mock<NamespacedKubernetesClient<V1Secret>>(MockBehavior.Strict);
            client.Setup(c => c.GetClient<V1Secret>()).Returns(secretClient.Object);

            secretClient
                .Setup(c => c.CreateAsync(It.IsAny<V1Secret>(), default))
                .Callback<V1Secret, CancellationToken>(
                (value, ct) =>
                {
                    Assert.NotNull(value);
                    Assert.Equal("abc", value.Metadata.Name);
                    Assert.NotNull(value.Data["tls.crt"]);
                    Assert.NotNull(value.Data["tls.key"]);
                })
                .Returns(Task.FromResult(secret))
                .Verifiable();

            var store = new KubernetesPairingRecordStore(client.Object, NullLogger<KubernetesPairingRecordStore>.Instance);

            await store.WriteAsync("abc", pairingRecord, default);

            secretClient.Verify();
        }
    }
}
