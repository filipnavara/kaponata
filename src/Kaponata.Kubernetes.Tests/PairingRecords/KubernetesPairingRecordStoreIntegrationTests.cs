// <copyright file="KubernetesPairingRecordStoreIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using k8s;
using Kaponata.iOS.Lockdown;
using Kaponata.Kubernetes.PairingRecords;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Kubernetes.Tests.PairingRecords
{
    /// <summary>
    /// Integration tests for the <see cref="KubernetesPairingRecordStore"/> class.
    /// </summary>
    public class KubernetesPairingRecordStoreIntegrationTests
    {
        private readonly ITestOutputHelper output;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesPairingRecordStoreIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The test output helper which will be used to log to xunit.
        /// </param>
        public KubernetesPairingRecordStoreIntegrationTests(ITestOutputHelper output)
        {
            this.output = output;
            this.loggerFactory = LogFactory.Create(output);
        }

        /// <summary>
        /// Tests the lifecycle of a pairing record, by adding, reading and deleting the record.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task PairingRecord_Lifecycle_Async()
        {
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var udid = "pairingrecord-lifecycle";
            var buid = Guid.NewGuid().ToString();

            var record = new PairingRecordGenerator().Generate(key, buid);

            using (var client = this.CreateKubernetesClient())
            {
                var store = new KubernetesPairingRecordStore(client, this.loggerFactory.CreateLogger<KubernetesPairingRecordStore>());

                // The record should not exist.
                Assert.Null(await store.ReadAsync(udid, default).ConfigureAwait(false));

                // Write the record; it can be retrieved afterwards
                await store.WriteAsync(udid, record, default).ConfigureAwait(false);
                var record2 = await store.ReadAsync(udid, default).ConfigureAwait(false);

                Assert.NotNull(record2);
                Assert.Equal(record.ToByteArray(), record2.ToByteArray());

                // Delete the record; it can no longer be retrieved afterwardss
                await store.DeleteAsync(udid, default).ConfigureAwait(false);
                Assert.Null(await store.ReadAsync(udid, default).ConfigureAwait(false));
            }
        }

        private KubernetesClient CreateKubernetesClient()
        {
            return new KubernetesClient(
                new KubernetesProtocol(
                    KubernetesClientConfiguration.BuildDefaultConfig(),
                    this.loggerFactory.CreateLogger<KubernetesProtocol>(),
                    this.loggerFactory),
                KubernetesOptions.Default,
                this.loggerFactory.CreateLogger<KubernetesClient>(),
                this.loggerFactory);
        }
    }
}
