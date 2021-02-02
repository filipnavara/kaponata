// <copyright file="ChildOperatorConfigurationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Operators;
using System;
using Xunit;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="ChildOperatorConfiguration"/> class.
    /// </summary>
    public class ChildOperatorConfigurationTests
    {
        /// <summary>
        /// The <see cref="ChildOperatorConfiguration"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("operatorName", () => new ChildOperatorConfiguration(null));
        }

        /// <summary>
        /// The <see cref="ChildOperatorConfiguration"/> constructor adds the default labels.
        /// </summary>
        [Fact]
        public void Constructor_AddsDefaultLabels()
        {
            var configuration = new ChildOperatorConfiguration("name");
            Assert.Collection(
                configuration.ChildLabels,
                l =>
                {
                    Assert.Equal(Annotations.ManagedBy, l.Key);
                    Assert.Equal("name", l.Value);
                });
        }
    }
}
