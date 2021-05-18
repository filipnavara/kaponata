// <copyright file="HangupRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.MobileImageMounter;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="HangupRequest"/> class.
    /// </summary>
    public class HangupRequestTests
    {
        /// <summary>
        /// <see cref="HangupRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var dict = new HangupRequest().ToDictionary();

            Assert.Equal("Hangup", Assert.Contains("Command", dict).ToObject());
        }
    }
}
