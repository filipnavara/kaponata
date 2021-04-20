// <copyright file="ImageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="Image"/> class.
    /// </summary>
    public class ImageTests
    {
        /// <summary>
        /// Creates a <see cref="Image"/> object and makes sure it is serialized correctly, by verifying the SHA hash of the serialized
        /// content.
        /// </summary>
        [Fact]
        public void SerializeTest()
        {
            var image = new Image()
            {
                Created = DateTime.Parse("2021-04-19T15:13:36.371862543Z").ToUniversalTime(),
                Architecture = "amd64",
                OS = "linux",
                Config = new ImageConfig()
                {
                    Env = new string[]
                    {
                        "PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin",
                    },
                },
                RootFS = new RootFS()
                {
                    Type = "layers",
                    DiffIDs = new string[]
                    {
                        "sha256:67e550a69d6b7dba6281cabea3cbda6ac0303ed77664cf0b54d3555e1c7ff8ba",
                    },
                },
                History = new History[]
                {
                    new History()
                    {
                        Created = DateTime.Parse("2021-04-19T15:13:36.371862543Z").ToUniversalTime(),
                        CreatedBy = "/bin/sh -c #(nop) ADD file:5d86cbc475b913f3286f196b51f14c52adc2df94fe240fac6c7004dd2175e1c2 in / ",
                    },
                },
            };

            var json = JsonSerializer.SerializeToUtf8Bytes(image);
            var sha = SHA1.Create();
            var hash = sha.ComputeHash(json);

            File.WriteAllBytes(@"C:\Users\frede\source\repos\kaponata\tmp\hello-oci\blobs\sha256\a06c12c3d92f5f15dc89fedf42a643f70d9d97b37299cdb95ec287e095c79fa4.json", json);
            Assert.Equal("727F5AF59F2F9A7C7A9A5BAFA75E88E8CA7BF89F", Convert.ToHexString(hash));
        }
    }
}
