// <copyright file="PlatformTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System.Text.Json;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="Platform"/> class.
    /// </summary>
    public class PlatformTests
    {
        /// <summary>
        /// <see cref="Platform"/> values can be deserialized correctly.
        /// </summary>
        [Fact]
        public void Platform_DeserializesCorrectly()
        {
            const string json = "{\"architecture\": \"amd64\",\"os\": \"linux\",\"os.features\": [\"win32k\"] }";
            var platform = JsonSerializer.Deserialize<Platform>(json);

            Assert.Equal("amd64", platform.Architecture);
            Assert.Equal("linux", platform.OS);
            Assert.Equal("win32k", Assert.Single(platform.OSFeatures));
            Assert.Null(platform.OSVersion);
            Assert.Null(platform.Variant);
        }
    }
}
