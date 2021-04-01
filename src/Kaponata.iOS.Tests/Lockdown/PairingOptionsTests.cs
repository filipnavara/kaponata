// <copyright file="PairingOptionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairingOptions"/> class.
    /// </summary>
    public class PairingOptionsTests
    {
        /// <summary>
        /// Tests the <see cref="PairingOptions.ToPropertyList"/> method.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var dict = new PairingOptions()
            {
                ExtendedPairingErrors = true,
            }.ToPropertyList();

            Assert.Collection(
                dict,
                k =>
                {
                    Assert.Equal("ExtendedPairingErrors", k.Key);
                    Assert.Equal(true, k.Value.ToObject());
                });
        }
    }
}
