// <copyright file="ModelDefinitionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Models;
using Xunit;

namespace Kaponata.Operator.Tests.Models
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
        }
    }
}
