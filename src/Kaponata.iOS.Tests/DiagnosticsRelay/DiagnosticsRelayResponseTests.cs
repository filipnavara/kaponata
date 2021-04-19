// <copyright file="DiagnosticsRelayResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.DiagnosticsRelay;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.DiagnosticsRelay
{
    /// <summary>
    /// Tests the <see cref="DiagnosticsRelayResponse"/> class.
    /// </summary>
    public class DiagnosticsRelayResponseTests
    {
        /// <summary>
        /// <see cref="DiagnosticsRelayResponse.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => DiagnosticsRelayResponse.Read(null));
        }

        /// <summary>
        /// <see cref="DiagnosticsRelayResponse.Read(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Status", "Success");

            var response = DiagnosticsRelayResponse.Read(dict);
            Assert.Equal(DiagnosticsRelayStatus.Success, response.Status);
        }
    }
}
