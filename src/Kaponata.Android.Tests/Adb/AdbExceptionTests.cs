// <copyright file="AdbExceptionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbException"/> class.
    /// </summary>
    public class AdbExceptionTests
    {
        /// <summary>
        /// The <see cref="AdbException.AdbException()"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var ex = new AdbException();
            Assert.Equal("An unexpected ADB error occurred.", ex.Message);
        }

        /// <summary>
        /// The <see cref="AdbException.AdbException(string)"/> constructor works.
        /// </summary>
        [Fact]
        public void Constructor_WithMessage_Works()
        {
            var ex = new AdbException("test.");
            Assert.Equal("test.", ex.Message);
        }
    }
}
