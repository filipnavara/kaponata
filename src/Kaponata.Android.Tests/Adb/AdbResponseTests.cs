// <copyright file="AdbResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbResponse"/> class.
    /// </summary>
    public class AdbResponseTests
    {
        /// <summary>
        /// The <see cref="AdbResponse.AdbResponse(AdbResponseStatus, string)"/> constructor validate the argments being passed.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            var response = new AdbResponse(AdbResponseStatus.OKAY, string.Empty);
            Assert.Equal(AdbResponseStatus.OKAY, response.Status);
            Assert.Equal(string.Empty, response.Message);

            response = new AdbResponse(AdbResponseStatus.FAIL, "ai");
            Assert.Equal(AdbResponseStatus.FAIL, response.Status);
            Assert.Equal("ai", response.Message);
        }

        /// <summary>
        /// The <see cref="AdbResponse.Success"/> property contains an <see cref="AdbResponseStatus.OKAY"/> status.
        /// </summary>
        [Fact]
        public void Success_HasOkayStatus()
        {
            var response = AdbResponse.Success;
            Assert.Equal(AdbResponseStatus.OKAY, response.Status);
            Assert.Equal(string.Empty, response.Message);
        }
    }
}
