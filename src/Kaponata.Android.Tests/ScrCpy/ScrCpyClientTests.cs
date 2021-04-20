// <copyright file="ScrCpyClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Kaponata.Android.ScrCpy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="ScrCpyClient"/> class.
    /// </summary>
    public class ScrCpyClientTests
    {
        /// <summary>
        /// The <see cref="ScrCpyClient.ScrCpyServer"/> returns a not null stream.
        /// </summary>
        [Fact]
        public void ScrCpyServer_NotNull()
        {
            var adbClient = new AdbClient(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);

            var adbDevice = new DeviceData()
            {
                Serial = "123",
            };
            var scrCpyClient = new ScrCpyClient(NullLogger<ScrCpyClient>.Instance, NullLoggerFactory.Instance, adbClient, adbDevice);

            Assert.NotNull(scrCpyClient.ScrCpyServer);
        }
    }
}
