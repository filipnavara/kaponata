// <copyright file="MuxerDeviceTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerDevice"/> class.
    /// </summary>
    public class MuxerDeviceTests
    {
        /// <summary>
        /// <see cref="MuxerDevice.ToString"/> returns the device UDID.
        /// </summary>
        [Fact]
        public void ToString_ReturnsUdid()
        {
            var device = new MuxerDevice()
            {
                Udid = "abc",
            };

            Assert.Equal("abc", device.ToString());
        }
    }
}
