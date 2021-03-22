// <copyright file="V1SecretExtensions.SignedCms.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.iOS.DeveloperProfiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;

namespace Kaponata.Kubernetes.DeveloperProfiles
{
    /// <summary>
    /// Provides methods for converting <see cref="SignedCms"/> objects to <see cref="V1Secret"/> objects and vice versa.
    /// </summary>
    public static partial class V1SecretExtensions
    {
        private const string SignedDataContent = "signedData";
        private const string SignedDataType = "kaponata.io/signedData";

        /// <summary>
        /// Converts this <see cref="V1Secret"/> to a <see cref="SignedCms"/>.
        /// </summary>
        /// <param name="secret">
        /// A <see cref="V1Secret"/> which represents CMS signed data.
        /// </param>
        /// <returns>
        /// The equivalent <see cref="SignedCms"/> value.
        /// </returns>
        public static SignedCms AsSignedCms(this V1Secret secret)
        {
            EnsureProvisioningProfileSecret(secret);

            SignedCms cms = new SignedCms();
            cms.Decode(secret.Data[SignedDataContent]);
            return cms;
        }

        /// <summary>
        /// Converts this <see cref="SignedCms"/> to a <see cref="V1Secret"/>.
        /// </summary>
        /// <param name="signedData">
        /// A <see cref="SignedCms"/> which represents a CMS signed data object.
        /// </param>
        /// <returns>
        /// The equivalent <see cref="V1Secret"/>.
        /// </returns>
        public static V1Secret AsSecret(this SignedCms signedData)
        {
            var secret = new V1Secret()
            {
                ApiVersion = V1Secret.KubeApiVersion,
                Kind = V1Secret.KubeKind,
                Type = SignedDataType,
                Metadata = new V1ObjectMeta()
                {
                    Labels = new Dictionary<string, string>(),
                },
                Data = new Dictionary<string, byte[]>(),
                Immutable = true,
            };

            secret.Data[SignedDataContent] = signedData.Encode();

            return secret;
        }

        private static void EnsureProvisioningProfileSecret(V1Secret secret)
        {
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (secret.Type != SignedDataType || secret.Data == null || !secret.Data.ContainsKey(SignedDataContent))
            {
                throw new InvalidDataException();
            }
        }
    }
}
