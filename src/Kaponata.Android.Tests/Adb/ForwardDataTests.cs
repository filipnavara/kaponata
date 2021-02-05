// <copyright file="ForwardDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="ForwardData"/> class.
    /// </summary>
    public class ForwardDataTests
    {
        /// <summary>
        /// The <see cref="ForwardData.FromString(string)"/> validates the input.
        /// </summary>
        [Fact]
        public void FromString_ValidatesInput()
        {
            Assert.Throws<ArgumentNullException>("value", () => ForwardData.FromString(null));
            Assert.Throws<ArgumentOutOfRangeException>("value", () => ForwardData.FromString("test1 test2"));
        }

        /// <summary>
        /// The <see cref="ForwardData.ToString"/> methods returns a string representation of the forward.
        /// </summary>
        [Fact]
        public void ToString_ReturnsStringRepresentation()
        {
            var forward = new ForwardData()
            {
                LocalSpec = ForwardSpec.Parse("tcp:4"),
                RemoteSpec = ForwardSpec.Parse("tcp:5"),
                SerialNumber = "1245",
            };

            Assert.Equal("1245 tcp:4 tcp:5", forward.ToString());
        }
    }
}
