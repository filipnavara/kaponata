// <copyright file="WebDriverDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverData"/> class.
    /// </summary>
    public class WebDriverDataTests
    {
        /// <summary>
        /// The <see cref="WebDriverData"/> constructors work correctly.
        /// </summary>
        [Fact]
        public void DefaultConstructor_Works()
        {
            Assert.Null(new WebDriverData().Data);
            Assert.Null(new WebDriverData(null).Data);

            var data = new object();
            Assert.Same(data, new WebDriverData(data).Data);
        }
    }
}
