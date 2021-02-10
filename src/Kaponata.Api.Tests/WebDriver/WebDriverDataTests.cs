// <copyright file="WebDriverDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="WebDriverResponse"/> class.
    /// </summary>
    public class WebDriverDataTests
    {
        /// <summary>
        /// The <see cref="WebDriverResponse"/> constructors work correctly.
        /// </summary>
        [Fact]
        public void DefaultConstructor_Works()
        {
            Assert.Null(new WebDriverResponse().Value);
            Assert.Null(new WebDriverResponse(null).Value);

            var data = new object();
            Assert.Same(data, new WebDriverResponse(data).Value);
        }
    }
}
