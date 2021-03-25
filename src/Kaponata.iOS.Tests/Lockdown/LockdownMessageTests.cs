// <copyright file="LockdownMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownMessage"/> class.
    /// </summary>
    public class LockdownMessageTests
    {
        /// <summary>
        /// <see cref="LockdownMessage.ToDictionary()"/> returns the correct value.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var dict = new LockdownMessage()
            {
                Request = "test",
            }.ToDictionary();

            Assert.Collection(
                dict,
                v =>
                {
                    Assert.Equal("Label", v.Key);
                    Assert.Equal("Kaponata.iOS", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("ProtocolVersion", v.Key);
                    Assert.Equal("2", v.Value.ToObject());
                },
                v =>
                {
                    Assert.Equal("Request", v.Key);
                    Assert.Equal("test", v.Value.ToObject());
                });
        }
    }
}
