// <copyright file="V1SecretExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Lockdown;
using Kaponata.Kubernetes.PairingRecords;
using System;
using Xunit;

namespace Kaponata.Kubernetes.Tests.PairingRecords
{
    /// <summary>
    /// Tests the <see cref="V1SecretExtensions"/> class.
    /// </summary>
    public class V1SecretExtensionsTests
    {
        /// <summary>
        /// <see cref="V1SecretExtensions.AsPairingRecord(V1Secret)"/> returns <see langword="null"/>
        /// when passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void AsPairingRecord_Null_ReturnsNull()
        {
            Assert.Null(V1SecretExtensions.AsPairingRecord(null));
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSecret(PairingRecord)"/> returns <see langword="null"/>
        /// when passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void AsSecret_Null_ReturnsNull()
        {
            Assert.Null(V1SecretExtensions.AsSecret(null));
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsPairingRecord(V1Secret?)"/> generates a secret which round-trips
        /// successfully.
        /// </summary>
        [Fact]
        public void AsSecret_Minimal_Roundtrip()
        {
            var systemBuid = Guid.NewGuid().ToString();
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var pairingRecord = PairingRecordGenerator.Generate(key, systemBuid);
            var secret = V1SecretExtensions.AsSecret(pairingRecord);

            Assert.NotNull(secret.Data["ca.crt"]);
            Assert.NotNull(secret.Data["ca.key"]);
            Assert.NotNull(secret.Data["tls.crt"]);
            Assert.NotNull(secret.Data["tls.key"]);
            Assert.NotNull(secret.Data["device.crt"]);

            Assert.NotNull(secret.Data["hostId"]);
            Assert.NotNull(secret.Data["systemBuid"]);

            var roundtripRecord = V1SecretExtensions.AsPairingRecord(secret);
            Assert.Equal(pairingRecord.DeviceCertificate, roundtripRecord.DeviceCertificate);
            Assert.Equal(pairingRecord.RootCertificate, roundtripRecord.RootCertificate);
            Assert.Equal(pairingRecord.RootPrivateKey.ExportPkcs8PrivateKey(), roundtripRecord.RootPrivateKey.ExportPkcs8PrivateKey());
            Assert.Equal(pairingRecord.HostCertificate, roundtripRecord.HostCertificate);
            Assert.Equal(pairingRecord.HostPrivateKey.ExportPkcs8PrivateKey(), roundtripRecord.HostPrivateKey.ExportPkcs8PrivateKey());
            Assert.Equal(pairingRecord.HostId, roundtripRecord.HostId);
            Assert.Equal(pairingRecord.SystemBUID, roundtripRecord.SystemBUID);
            Assert.Null(roundtripRecord.EscrowBag);
            Assert.Null(roundtripRecord.WiFiMacAddress);
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsPairingRecord(V1Secret?)"/> generates a secret which round-trips
        /// successfully.
        /// </summary>
        [Fact]
        public void AsSecret_Complete_Roundtrip()
        {
            var systemBuid = Guid.NewGuid().ToString();
            var key = Convert.FromBase64String(
                "LS0tLS1CRUdJTiBSU0EgUFVCTElDIEtFWS0tLS0tCk1JR0pBb0dCQUorNXVIQjJycllw" +
                "VEt4SWNGUnJxR1ZqTHRNQ2wyWHhmTVhJeEhYTURrM01jV2hxK2RtWkcvWW0KeDJuTGZq" +
                "WWJPeUduQ1BxQktxcUU5Q2tyQy9DUi9mTlgwNjJqMU1pUHJYY2RnQ0tiNzB2bmVlMFNF" +
                "T2FmNVhEQworZWFZeGdjWTYvbjBXODNrSklXMGF0czhMWmUwTW9XNXpXSTh6cnM4eDIw" +
                "UFFJK1RGU1p4QWdNQkFBRT0KLS0tLS1FTkQgUlNBIFBVQkxJQyBLRVktLS0tLQo=");

            var pairingRecord = PairingRecordGenerator.Generate(key, systemBuid);
            pairingRecord.EscrowBag = new byte[] { 1, 2, 3, 4 };
            pairingRecord.WiFiMacAddress = "aa:bb:cc:dd";

            var secret = V1SecretExtensions.AsSecret(pairingRecord);

            Assert.NotNull(secret.Data["ca.crt"]);
            Assert.NotNull(secret.Data["ca.key"]);
            Assert.NotNull(secret.Data["tls.crt"]);
            Assert.NotNull(secret.Data["tls.key"]);
            Assert.NotNull(secret.Data["device.crt"]);

            Assert.NotNull(secret.Data["hostId"]);
            Assert.NotNull(secret.Data["systemBuid"]);

            var roundtripRecord = V1SecretExtensions.AsPairingRecord(secret);
            Assert.Equal(pairingRecord.DeviceCertificate, roundtripRecord.DeviceCertificate);
            Assert.Equal(pairingRecord.RootCertificate, roundtripRecord.RootCertificate);
            Assert.Equal(pairingRecord.RootPrivateKey.ExportPkcs8PrivateKey(), roundtripRecord.RootPrivateKey.ExportPkcs8PrivateKey());
            Assert.Equal(pairingRecord.HostCertificate, roundtripRecord.HostCertificate);
            Assert.Equal(pairingRecord.HostPrivateKey.ExportPkcs8PrivateKey(), roundtripRecord.HostPrivateKey.ExportPkcs8PrivateKey());
            Assert.Equal(pairingRecord.HostId, roundtripRecord.HostId);
            Assert.Equal(pairingRecord.SystemBUID, roundtripRecord.SystemBUID);
            Assert.Equal(pairingRecord.EscrowBag, roundtripRecord.EscrowBag);
            Assert.Equal(pairingRecord.WiFiMacAddress, roundtripRecord.WiFiMacAddress);
        }
    }
}
