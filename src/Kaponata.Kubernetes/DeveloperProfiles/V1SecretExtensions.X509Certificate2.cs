// <copyright file="V1SecretExtensions.X509Certificate2.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kaponata.Kubernetes.DeveloperProfiles
{
    /// <summary>
    /// Provides methods for converting <see cref="V1Secret"/> objects to <see cref="X509Certificate2"/> objects
    /// and vice versa.
    /// </summary>
    public static partial class V1SecretExtensions
    {
        private const string TlsCertificate = "tls.crt";
        private const string TlsPrivateKey = "tls.key";
        private const string TlsType = "kubernetes.io/tls";

        /// <summary>
        /// Converts this <see cref="V1Secret"/> to a <see cref="X509Certificate2"/>.
        /// </summary>
        /// <param name="secret">
        /// A <see cref="V1Secret"/> which represents a Kubernetes TLS secret.
        /// </param>
        /// <returns>
        /// The equivalent <see cref="X509Certificate2"/> value.
        /// </returns>
        public static X509Certificate2 AsX509Certificate2(this V1Secret secret)
        {
            EnsureTlsSecret(secret);

            // Both DER-encoded
            return X509Certificate2.CreateFromPem(
                Encoding.UTF8.GetString(secret.Data[TlsCertificate]),
                Encoding.UTF8.GetString(secret.Data[TlsPrivateKey]));
        }

        /// <summary>
        /// Converts this <see cref="X509Certificate2"/> to a <see cref="V1Secret"/>.
        /// </summary>
        /// <param name="certificate">
        /// A <see cref="X509Certificate2"/> which represents a TLS certificate.
        /// </param>
        /// <returns>
        /// The equivalent <see cref="V1Secret"/>.
        /// </returns>
        public static V1Secret AsSecret(this X509Certificate2 certificate)
        {
            var secret = new V1Secret()
            {
                ApiVersion = V1Secret.KubeApiVersion,
                Kind = V1Secret.KubeKind,
                Type = TlsType,
                Metadata = new V1ObjectMeta()
                {
                    // Must conform to a DNS subdomain naming rules, so all lowercase ASCII.
                    Name = certificate.Thumbprint.ToLowerInvariant(),
                    Labels = new Dictionary<string, string>(),
                },
                Data = new Dictionary<string, byte[]>(),
                Immutable = true,
            };

            var certificateAsPem =
                PemEncoding.Write(
                    "CERTIFICATE",
                    certificate.Export(X509ContentType.Cert));

            secret.Data[TlsCertificate] = Encoding.UTF8.GetBytes(certificateAsPem);

            if (certificate.PrivateKey != null)
            {
                var keyAsPem =
                    PemEncoding.Write(
                        "PRIVATE KEY",
                        certificate.PrivateKey.ExportPkcs8PrivateKey());

                secret.Data[TlsPrivateKey] = Encoding.UTF8.GetBytes(keyAsPem);
            }

            return secret;
        }

        private static void EnsureTlsSecret(V1Secret secret)
        {
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (secret.Type != TlsType || secret.Data == null || !secret.Data.ContainsKey(TlsCertificate) || !secret.Data.ContainsKey(TlsPrivateKey))
            {
                throw new InvalidDataException($"The secret {secret?.Metadata?.Name} is not a valid TLS secret.");
            }
        }
    }
}
