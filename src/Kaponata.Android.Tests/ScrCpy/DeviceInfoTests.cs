// <copyright file="DeviceInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.ScrCpy;
using System.IO;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="DeviceInfo"/> class.
    /// </summary>
    public class DeviceInfoTests
    {
        /// <summary>
        /// The <see cref="DeviceInfo.Read(System.Span{byte})"/> method creates a <see cref="DeviceInfo"/> struct from the data.
        /// </summary>
        [Fact]
        public void Read_ReadsHeader()
        {
            var deviceInfoData = File.ReadAllBytes("ScrCpy/device_info.bin");
            var deviceInfo = DeviceInfo.Read(deviceInfoData);

            Assert.Equal("SM-G950F", deviceInfo.DeviceName);
            Assert.Equal(2960, deviceInfo.Width);
            Assert.Equal(1440, deviceInfo.Height);
        }
    }
}
