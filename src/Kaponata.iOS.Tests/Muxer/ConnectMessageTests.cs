// <copyright file="ConnectMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="ConnectMessage"/> class.
    /// </summary>
    public class ConnectMessageTests
    {
        /// <summary>
        /// THe <see cref="ConnectMessage.ToPropertyList"/> method returns the correct values.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var message = new ConnectMessage()
            {
                DeviceID = 1,
                PortNumber = 2,
            };

            var dict = message.ToPropertyList();

            Assert.Collection(
                dict,
                (v) =>
                {
                    Assert.Equal("BundleID", v.Key);
                    Assert.Equal("Kaponata.iOS", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("ClientVersionString", v.Key);
                    Assert.Equal(ThisAssembly.AssemblyInformationalVersion, v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("DeviceID", v.Key);
                    Assert.Equal(1, v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("MessageType", v.Key);
                    Assert.Equal("Connect", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("ProgName", v.Key);
                    Assert.Equal("Kaponata.iOS", v.Value.ToObject());
                },
                (v) =>
                {
                    Assert.Equal("PortNumber", v.Key);
                    Assert.Equal(2, v.Value.ToObject());
                });
        }
    }
}
