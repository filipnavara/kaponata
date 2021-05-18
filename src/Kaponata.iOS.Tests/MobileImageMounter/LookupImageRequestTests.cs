// <copyright file="LookupImageRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.MobileImageMounter;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="LookupImageRequest"/> class.
    /// </summary>
    public class LookupImageRequestTests
    {
        /// <summary>
        /// <see cref="LookupImageRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var dict = new LookupImageRequest()
            {
                ImageType = "Developer",
            }.ToDictionary();

            Assert.Equal("LookupImage", Assert.Contains("Command", dict).ToObject());
            Assert.Equal("Developer", Assert.Contains("ImageType", dict).ToObject());
        }
    }
}
