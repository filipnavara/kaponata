// <copyright file="V1SecretExtensions.PairingRecord.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.Lockdown;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kaponata.Kubernetes.PairingRecords
{
    /// <summary>
    /// Provides methods for storing <see cref="PairingRecord"/> objects as <see cref="V1Secret"/> objects.
    /// </summary>
    public static class V1SecretExtensions
    {
        private const string TlsCertificateKey = "tls.crt";
        private const string TlsPrivateKey = "tls.key";

        private const string CaCertificateKey = "ca.crt";
        private const string CaPrivateKey = "ca.key";

        private const string DeviceCertificateKey = "device.crt";

        private const string EscrowBagKey = "escrow.key";
        private const string HostIdKey = "hostId";
        private const string SystemBuidKey = "systemBuid";
        private const string WifiMacAddressKey = "wifiMacAddress";

        private const string TlsType = "kubernetes.io/tls";

        /// <summary>
        /// Converts a <see cref="PairingRecord"/> to a <see cref="V1Secret"/> object.
        /// </summary>
        /// <param name="pairingRecord">
        /// The pairing record to convert.
        /// </param>
        /// <returns>
        /// A <see cref="V1Secret"/> which represents the <see cref="PairingRecord"/>.
        /// </returns>
        [return: NotNullIfNotNull("pairingRecord")]
        public static V1Secret? AsSecret(this PairingRecord? pairingRecord)
        {
            if (pairingRecord == null)
            {
                return null;
            }

            var secret = new V1Secret()
            {
                ApiVersion = V1Secret.KubeApiVersion,
                Kind = V1Secret.KubeKind,
                Type = TlsType,
                Metadata = new V1ObjectMeta(),
                Data = new Dictionary<string, byte[]>(),
                Immutable = true,
            };

            secret.Data[TlsCertificateKey] =
                Encoding.UTF8.GetBytes(
                    PemEncoding.Write(
                        "CERTIFICATE",
                        pairingRecord.HostCertificate.Export(X509ContentType.Cert)));

            secret.Data[TlsPrivateKey] =
                Encoding.UTF8.GetBytes(
                    PemEncoding.Write(
                        "PRIVATE KEY",
                        pairingRecord.HostPrivateKey.ExportPkcs8PrivateKey()));

            secret.Data[CaCertificateKey] =
                Encoding.UTF8.GetBytes(
                    PemEncoding.Write(
                        "CERTIFICATE",
                        pairingRecord.RootCertificate.Export(X509ContentType.Cert)));

            secret.Data[CaPrivateKey] =
                Encoding.UTF8.GetBytes(
                    PemEncoding.Write(
                        "PRIVATE KEY",
                        pairingRecord.RootPrivateKey.ExportPkcs8PrivateKey()));

            secret.Data[DeviceCertificateKey] =
                Encoding.UTF8.GetBytes(
                    PemEncoding.Write(
                        "CERTIFICATE",
                        pairingRecord.DeviceCertificate.Export(X509ContentType.Cert)));

            secret.Data[EscrowBagKey] = pairingRecord.EscrowBag;
            secret.Data[HostIdKey] = Encoding.UTF8.GetBytes(pairingRecord.HostId);
            secret.Data[SystemBuidKey] = Encoding.UTF8.GetBytes(pairingRecord.SystemBUID);
            secret.Data[WifiMacAddressKey] = pairingRecord.WiFiMacAddress == null ? null : Encoding.UTF8.GetBytes(pairingRecord.WiFiMacAddress);

            return secret;
        }

        /// <summary>
        /// Converts a <see cref="V1Secret"/> to a <see cref="PairingRecord"/>.
        /// </summary>
        /// <param name="secret">
        /// A <see cref="V1Secret"/> which represents a pairing record.
        /// </param>
        /// <returns>
        /// An equivalent <see cref="PairingRecord"/>.
        /// </returns>
        public static PairingRecord? AsPairingRecord(this V1Secret? secret)
        {
            if (secret == null)
            {
                return null;
            }

            return new PairingRecord()
            {
                HostCertificate = new X509Certificate2(secret.Data[TlsCertificateKey]),
                HostPrivateKey = DeserializePrivateKey(secret.Data[TlsPrivateKey]),

                RootCertificate = new X509Certificate2(secret.Data[CaCertificateKey]),
                RootPrivateKey = DeserializePrivateKey(secret.Data[CaPrivateKey]),

                DeviceCertificate = new X509Certificate2(secret.Data[DeviceCertificateKey]),

                EscrowBag = secret.Data[EscrowBagKey] != null && secret.Data[EscrowBagKey].Length > 0 ? secret.Data[EscrowBagKey] : null,
                HostId = Encoding.UTF8.GetString(secret.Data[HostIdKey]),
                SystemBUID = Encoding.UTF8.GetString(secret.Data[SystemBuidKey]),
                WiFiMacAddress = secret.Data.ContainsKey(WifiMacAddressKey) && secret.Data[WifiMacAddressKey] != null && secret.Data[WifiMacAddressKey].Length > 0 ? Encoding.UTF8.GetString(secret.Data[WifiMacAddressKey]) : null,
            };
        }

        private static RSA DeserializePrivateKey(byte[] data)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(Encoding.UTF8.GetString(data));
            return rsa;
        }
    }
}
