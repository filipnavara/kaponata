// <copyright file="ScrCpyDeviceInfoTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Kaponata.Android.ScrCpy;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="ScrCpyDeviceInfo"/> class.
    /// </summary>
    public class ScrCpyDeviceInfoTests
    {
        /// <summary>
        /// The <see cref="ScrCpyDeviceInfo.Read(System.Span{byte})"/> method creates a <see cref="ScrCpyDeviceInfo"/> struct from the data.
        /// </summary>
        [Fact]
        public void Read_ReadsHeader()
        {
            var deviceInfoData = File.ReadAllBytes("ScrCpy/device_info.bin");
            var deviceInfo = ScrCpyDeviceInfo.Read(deviceInfoData);

            Assert.Equal("SM-G950F", deviceInfo.DeviceName);
            Assert.Equal(2960, deviceInfo.Width);
            Assert.Equal(1440, deviceInfo.Height);
        }

        /// <summary>
        /// The <see cref="ScrCpyDeviceInfo.BinarySize"/> returns the binary size.
        /// </summary>
        [Fact]
        public void BinarySize()
        {
            Assert.Equal(68, ScrCpyDeviceInfo.BinarySize);
        }
    }
}
