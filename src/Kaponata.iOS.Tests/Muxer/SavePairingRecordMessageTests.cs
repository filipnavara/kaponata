﻿// <copyright file="SavePairingRecordMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="SavePairingRecordMessage"/> class.
    /// </summary>
    public class SavePairingRecordMessageTests
    {
        /// <summary>
        /// <see cref="SavePairingRecordMessage.ToPropertyList"/> works correctly.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            byte[] data = new byte[] { 1, 2, 3 };

            var dict = new SavePairingRecordMessage()
            {
                PairRecordID = "abc",
                PairRecordData = data,
            }.ToPropertyList();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("BundleID", k.Key);
                    Assert.Equal("Kaponata.iOS", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ClientVersionString", k.Key);
                    Assert.Equal(ThisAssembly.AssemblyInformationalVersion, k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("MessageType", k.Key);
                    Assert.Equal("SavePairRecord", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ProgName", k.Key);
                    Assert.Equal("Kaponata.iOS", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("PairRecordID", k.Key);
                    Assert.Equal("abc", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("PairRecordData", k.Key);
                    Assert.Equal(data, k.Value.ToObject());
                });
        }
    }
}
