// <copyright file="ModelDefinitionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Models;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Models
{
    /// <summary>
    /// Tests the <see cref="ModelDefinitions"/> class.
    /// </summary>
    public class ModelDefinitionsTests
    {
        /// <summary>
        /// The <see cref="ModelDefinitions.MobileDevice"/> property returns the correct value.
        /// </summary>
        [Fact]
        public void MobileDevice_Works()
        {
            Assert.NotNull(ModelDefinitions.MobileDevice);
            Assert.Equal("mobiledevices.kaponata.io", ModelDefinitions.MobileDevice.Metadata.Name);
        }

        /// <summary>
        /// The <see cref="ModelDefinitions.WebDriverSession"/> property returns the correct value.
        /// </summary>
        [Fact]
        public void WebDriverSession_Works()
        {
            Assert.NotNull(ModelDefinitions.WebDriverSession);
            Assert.Equal("webdriversessions.kaponata.io", ModelDefinitions.WebDriverSession.Metadata.Name);
        }
    }
}
