// <copyright file="MountImageRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.MobileImageMounter;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="MountImageRequest"/> class.
    /// </summary>
    public class MountImageRequestTests
    {
        /// <summary>
        /// <see cref="MountImageRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var request = new MountImageRequest()
            {
                ImagePath = "/private/var/mobile/Media/PublicStaging/staging.dimage",
                ImageType = "Developer",
                ImageSignature = new byte[] { 1, 2, 3, 4 },
            }.ToDictionary();

            Assert.Equal("MountImage", Assert.Contains<string, NSObject>("Command", request).ToObject());
            Assert.Equal("/private/var/mobile/Media/PublicStaging/staging.dimage", Assert.Contains<string, NSObject>("ImagePath", request).ToObject());
            Assert.Equal("Developer", Assert.Contains<string, NSObject>("ImageType", request).ToObject());
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, Assert.Contains<string, NSObject>("ImageSignature", request).ToObject());
        }
    }
}
