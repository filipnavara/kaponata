// <copyright file="KindMetadataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes;
using System;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="KindMetadata"/> class.
    /// </summary>
    public class KindMetadataTests
    {
        /// <summary>
        /// The <see cref="KindMetadata"/> validates its arguments.
        /// </summary>
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("group", () => new KindMetadata("group", "version", "kind", "plural"));
            Assert.Throws<ArgumentNullException>("version", () => new KindMetadata("group", null, "kind", "plural"));
            Assert.Throws<ArgumentNullException>("kind", () => new KindMetadata("group", "version", null, "plural"));
            Assert.Throws<ArgumentNullException>("plural", () => new KindMetadata("group", "version", "kind", null));
        }

        /// <summary>
        /// The <see cref="KindMetadata"/> intializes the object properties.
        /// </summary>
        public void Constructor_SetsProperties()
        {
            var meta = new KindMetadata("group", "version", "kind", "plural");
            Assert.Equal("group", meta.Group);
            Assert.Equal("version", meta.Version);
            Assert.Equal("kind", meta.Kind);
            Assert.Equal("plural", meta.Plural);

            Assert.Equal("group/version", meta.ApiVersion);
        }
    }
}
