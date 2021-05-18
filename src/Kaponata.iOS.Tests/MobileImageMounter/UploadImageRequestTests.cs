// <copyright file="UploadImageRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.MobileImageMounter;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="UploadImageRequest"/> class.
    /// </summary>
    public class UploadImageRequestTests
    {
        /// <summary>
        /// <see cref="UploadImageRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var request = new UploadImageRequest()
            {
                ImageSignature = new byte[] { 1, 2, 3, 4 },
                ImageSize = 4,
                ImageType = "Developer",
            }.ToDictionary();

            Assert.Equal("ReceiveBytes", Assert.Contains("Command", request).ToObject());
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, Assert.Contains("ImageSignature", request).ToObject());
            Assert.Equal(4, Assert.Contains("ImageSize", request).ToObject());
            Assert.Equal("Developer", Assert.Contains("ImageType", request).ToString());
        }
    }
}
