﻿// <copyright file="DiagnosticsRelayExceptionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DiagnosticsRelay;
using Xunit;

namespace Kaponata.iOS.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayException"/> class.
    /// </summary>
    public class DiagnosticsRelayExceptionTests
    {
        /// <summary>
        /// The <see cref="DiagnosticsRelayException"/> constructor works correctly.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var ex = new DiagnosticsRelayException(DiagnosticsRelayStatus.Failure);

            Assert.Equal("An error occurred while sending a diagnostics relay request: Failure.", ex.Message);
            Assert.Equal(DiagnosticsRelayStatus.Failure, ex.Status);
        }
    }
}
