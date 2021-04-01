﻿// <copyright file="PairRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairRequest"/> class.
    /// </summary>
    public class PairRequestTests
    {
        /// <summary>
        /// <see cref="PairRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionaryList_WithoutOptions_Works()
        {
            var dict = new PairRequest()
            {
                PairRecord = new PairingRecord(),
            }.ToDictionary();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("Label", k.Key);
                    Assert.Equal("Kaponata.iOS", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ProtocolVersion", k.Key);
                    Assert.Equal("2", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("PairRecord", k.Key);
                    Assert.IsType<NSDictionary>(k.Value);
                });
        }

        /// <summary>
        /// <see cref="PairRequest.ToDictionary"/> works correctly.
        /// </summary>
        [Fact]
        public void ToDictionaryList_WithOptions_Works()
        {
            var dict = new PairRequest()
            {
                PairRecord = new PairingRecord(),
                PairingOptions = new PairingOptions() { ExtendedPairingErrors = true },
            }.ToDictionary();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("Label", k.Key);
                    Assert.Equal("Kaponata.iOS", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("ProtocolVersion", k.Key);
                    Assert.Equal("2", k.Value.ToObject());
                },
                k =>
                {
                    Assert.Equal("PairRecord", k.Key);
                    Assert.IsType<NSDictionary>(k.Value);
                },
                k =>
                {
                    Assert.Equal("PairingOptions", k.Key);
                    Assert.IsType<NSDictionary>(k.Value);
                });
        }
    }
}
