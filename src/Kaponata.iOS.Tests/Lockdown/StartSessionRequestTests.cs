// <copyright file="StartSessionRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="StartSessionRequest" /> class.
    /// </summary>
    public class StartSessionRequestTests
    {
        /// <summary>
        /// <see cref="StartSessionRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var dict = new StartSessionRequest()
            {
                HostID = "hostId",
                SystemBUID = "systemBuid",
            }.ToDictionary();

            Assert.Collection(
                dict,
                (k) =>
                {
                    Assert.Equal("Label", k.Key);
                    Assert.Equal("Kaponata.iOS", k.Value.ToObject());
                },
                (k) =>
                {
                    Assert.Equal("ProtocolVersion", k.Key);
                    Assert.Equal("2", k.Value.ToObject());
                },
                (k) =>
                {
                    Assert.Equal("HostID", k.Key);
                    Assert.Equal("hostId", k.Value.ToObject());
                },
                (k) =>
                {
                    Assert.Equal("SystemBUID", k.Key);
                    Assert.Equal("systemBuid", k.Value.ToObject());
                });
        }
    }
}
