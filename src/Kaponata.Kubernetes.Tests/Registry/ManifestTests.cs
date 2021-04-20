// <copyright file="ManifestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="Manifest"/> class.
    /// </summary>
    public class ManifestTests
    {
        /// <summary>
        /// Creates a <see cref="Manifest"/> object and makes sure it is serialized correctly, by verifying the SHA hash of the serialized
        /// content.
        /// </summary>
        [Fact]
        public void SerializeTest()
        {
            var manifest = new Manifest()
            {
                SchemaVersion = 2,
                Config = new Descriptor()
                {
                    MediaType = "application/vnd.oci.image.config.v1+json",
                    Digest = "sha256:a06c12c3d92f5f15dc89fedf42a643f70d9d97b37299cdb95ec287e095c79fa4",
                    Size = 451,
                },
                Layers = new Descriptor[]
                {
                    new Descriptor()
                    {
                        MediaType = "application/vnd.oci.image.layer.v1.tar+gzip",
                        Digest = "sha256:0aad37b1b216491367c41e6781df537c4c8876690b302310995db3101005da28",
                        Size = 131,
                    },
                },
            };

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            var json = JsonSerializer.SerializeToUtf8Bytes(manifest, options);
            File.WriteAllBytes(@"C:\Users\frede\source\repos\kaponata\tmp\hello-oci\blobs\sha256\2a4fd57a4438bd9d689fc4108e414384330bd78fc91086f49a7f89443515d356.json", json);

            var sha = SHA256.Create();
            var hash = sha.ComputeHash(json);

            Assert.Equal("2a4fd57a4438bd9d689fc4108e414384330bd78fc91086f49a7f89443515d356", Convert.ToHexString(hash).ToLowerInvariant());
        }
    }
}
