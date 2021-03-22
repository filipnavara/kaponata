// <copyright file="V1SecretExtensionsTests.X509Certificate2.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.DeveloperProfiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="X509Certificate2"/>-related methods.
    /// </summary>
    public partial class V1SecretExtensionsTests
    {
        /// <summary>
        /// Returns tests data for the <see cref="AsX509Certificate2_ValidatesData(Type, V1Secret)"/> method.
        /// </summary>
        /// <returns>
        /// Tests data for the <see cref="AsX509Certificate2_ValidatesData(Type, V1Secret)"/> method.
        /// </returns>
        public static IEnumerable<object[]> AsX509Certificate2_ValidatesData_TestData()
        {
            yield return new object[]
            {
                typeof(ArgumentNullException),
                (V1Secret)null,
            };

            yield return new object[]
            {
                typeof(InvalidDataException),
                new V1Secret(),
            };

            yield return new object[]
            {
                typeof(InvalidDataException),
                new V1Secret()
                {
                    Type = "abc",
                },
            };

            yield return new object[]
            {
                typeof(InvalidDataException),
                new V1Secret()
                {
                    Type = "kubernetes.io/tls",
                },
            };

            yield return new object[]
            {
                typeof(InvalidDataException),
                new V1Secret()
                {
                    Type = "kubernetes.io/tls",
                    Data = new Dictionary<string, byte[]>(),
                },
            };

            yield return new object[]
            {
                typeof(InvalidDataException),
                new V1Secret()
                {
                    Type = "kubernetes.io/tls",
                    Data = new Dictionary<string, byte[]>()
                    {
                        { "tls.crt", Array.Empty<byte>() },
                    },
                },
            };
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSecret(X509Certificate2)"/> throws an exception when passed invalid data.
        /// </summary>
        /// <param name="expectedException">
        /// The expected exception.
        /// </param>
        /// <param name="secret">
        /// The malformed secret.
        /// </param>
        [Theory]
        [MemberData(nameof(AsX509Certificate2_ValidatesData_TestData))]
        public void AsX509Certificate2_ValidatesData(Type expectedException, V1Secret secret)
        {
            Assert.Throws(expectedException, () => secret.AsX509Certificate2());
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsX509Certificate2(V1Secret)"/> returns a valid <see cref="X509Certificate2"/>.
        /// </summary>
        [Fact]
        public void AsX509Certificate2_Works()
        {
            // https://github.com/kubernetes-sigs/cluster-api/blob/master/docs/book/src/tasks/certs/using-custom-certificates.md
            V1Secret secret = new V1Secret()
            {
                Data = new Dictionary<string, byte[]>(),
                Type = "kubernetes.io/tls",
            };

            secret.Data["tls.crt"] = File.ReadAllBytes("DeveloperProfiles/tls.crt");
            secret.Data["tls.key"] = File.ReadAllBytes("DeveloperProfiles/tls.key");

            var certificate = secret.AsX509Certificate2();

            Assert.NotNull(certificate);
            Assert.Equal("CN=Test", certificate.Subject);
            Assert.True(certificate.HasPrivateKey);
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSecret(X509Certificate2)"/> works correctly.
        /// </summary>
        [Fact]
        public void AsSecret_Works()
        {
            var certificate = X509Certificate2.CreateFromPem(
                File.ReadAllText("DeveloperProfiles/tls.crt"),
                File.ReadAllText("DeveloperProfiles/tls.key"));

            var secret = certificate.AsSecret();

            Assert.Equal("v1", secret.ApiVersion);
            Assert.Equal("Secret", secret.Kind);
            Assert.Equal("kubernetes.io/tls", secret.Type);
            Assert.NotNull(secret.Metadata);
            Assert.Equal("c973377d7991bebbe3d85dffe838a61f5283d621", secret.Metadata.Name);
            Assert.NotNull(secret.Metadata.Labels);
            Assert.Empty(secret.Metadata.Labels);
            Assert.Equal(File.ReadAllText("DeveloperProfiles/tls.crt"), Encoding.UTF8.GetString(secret.Data["tls.crt"]), ignoreLineEndingDifferences: true);
            Assert.Equal(File.ReadAllText("DeveloperProfiles/tls.key"), Encoding.UTF8.GetString(secret.Data["tls.key"]), ignoreLineEndingDifferences: true);
        }
    }
}
