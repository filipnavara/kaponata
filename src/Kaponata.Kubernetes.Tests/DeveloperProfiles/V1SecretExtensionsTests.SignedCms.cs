// <copyright file="V1SecretExtensionsTests.SignedCms.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.DeveloperProfiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="SignedCms"/>-related methods in <see cref="V1SecretExtensions"/>.
    /// </summary>
    public partial class V1SecretExtensionsTests
    {
        /// <summary>
        /// Returns tests data for the <see cref="AsSignedCms_ValidatesData(Type, V1Secret)"/> method.
        /// </summary>
        /// <returns>
        /// Tests data for the <see cref="AsSignedCms_ValidatesData(Type, V1Secret)"/> method.
        /// </returns>
        public static IEnumerable<object[]> AsSignedCms_ValidatesData_TestData()
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
                    Type = "kaponata.io/signedData",
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
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSignedCms(V1Secret)"/> throws an exception when passed invalid data.
        /// </summary>
        /// <param name="expectedException">
        /// The expected exception.
        /// </param>
        /// <param name="secret">
        /// The malformed secret.
        /// </param>
        [Theory]
        [MemberData(nameof(AsX509Certificate2_ValidatesData_TestData))]
        public void AsSignedCms_ValidatesData(Type expectedException, V1Secret secret)
        {
            Assert.Throws(expectedException, () => secret.AsX509Certificate2());
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSignedCms(V1Secret)"/> returns a valid <see cref="SignedCms"/> object.
        /// </summary>
        [Fact]
        public void AsSignedCms_Works()
        {
            // https://github.com/kubernetes-sigs/cluster-api/blob/master/docs/book/src/tasks/certs/using-custom-certificates.md
            V1Secret secret = new V1Secret()
            {
                Data = new Dictionary<string, byte[]>(),
                Type = "kaponata.io/signedData",
            };

            secret.Data["signedData"] = File.ReadAllBytes("DeveloperProfiles/test.mobileprovision");

            var cms = secret.AsSignedCms();

            var certificate = cms.Certificates[0];
            Assert.NotNull(certificate);
            Assert.Equal("CN=Apple iPhone Certification Authority, OU=Apple Certification Authority, O=Apple Inc., C=US", certificate.Subject);
            Assert.False(certificate.HasPrivateKey);
        }

        /// <summary>
        /// <see cref="V1SecretExtensions.AsSecret(SignedCms)"/> works correctly.
        /// </summary>
        [Fact]
        public void SignedDataAsSecret_Works()
        {
            var signedData = new SignedCms();
            signedData.Decode(File.ReadAllBytes("DeveloperProfiles/test.mobileprovision"));

            var secret = signedData.AsSecret();

            Assert.Equal("v1", secret.ApiVersion);
            Assert.Equal("Secret", secret.Kind);
            Assert.Equal("kaponata.io/signedData", secret.Type);
            Assert.NotNull(secret.Metadata);
            Assert.NotNull(secret.Metadata.Labels);
            Assert.Empty(secret.Metadata.Labels);
            Assert.Equal(File.ReadAllBytes("DeveloperProfiles/test.mobileprovision"), secret.Data["signedData"]);
        }
    }
}
