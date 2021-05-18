// <copyright file="ImageRegistryClientConfigurationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="ImageRegistryClientConfiguration"/> class.
    /// </summary>
    public class ImageRegistryClientConfigurationTests
    {
        /// <summary>
        /// The <see cref="ImageRegistryClientConfiguration"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var configuration = new ImageRegistryClientConfiguration("test", 1);
            Assert.Equal("test", configuration.ServiceName);
            Assert.Equal(1, configuration.Port);
        }
    }
}
