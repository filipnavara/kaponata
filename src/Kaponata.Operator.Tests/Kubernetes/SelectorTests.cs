// <copyright file="SelectorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="Selector"/> class.
    /// </summary>
    public class SelectorTests
    {
        /// <summary>
        /// <see cref="Selector.Create(Dictionary{string, string})"/> returns <see langword="null"/> values
        /// when passed empty selectors.
        /// </summary>
        [Fact]
        public void CreateEmptySelector_ReturnsNull()
        {
            Assert.Null(Selector.Create(null));
            Assert.Null(Selector.Create(new Dictionary<string, string>()));
        }

        /// <summary>
        /// <see cref="Selector.Create(Dictionary{string, string})"/> returns the correct value.
        /// </summary>
        [Fact]
        public void CreateSelector_ReturnsString()
        {
            Assert.Equal("kubernetes.io/os=android", Selector.Create(new () { { Annotations.Os, Annotations.OperatingSystem.Android } }));
            Assert.Equal(
                "kubernetes.io/os=android,kubernetes.io/arch=arm64",
                Selector.Create(
                    new ()
                    {
                        { Annotations.Os, Annotations.OperatingSystem.Android },
                        { Annotations.Arch, Annotations.Architecture.Arm64 },
                    }));
        }
    }
}
