// <copyright file="PairResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="PairResponse"/> class.
    /// </summary>
    public class PairResponseTests
    {
        /// <summary>
        /// <see cref="PairResponse.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => PairResponse.Read(null));
        }

        /// <summary>
        /// <see cref="PairResponse.Read(NSDictionary)"/> parses the escrow bag.
        /// </summary>
        [Fact]
        public void Read_WithEscrowBag_Works()
        {
            var dict = new NSDictionary();
            byte[] data = new byte[] { 1, 2, 3, 4 };

            dict.Add("EscrowBag", data);

            var value = PairResponse.Read(dict);

            Assert.Null(value.Error);
            Assert.Equal(data, value.EscrowBag);
        }

        /// <summary>
        /// <see cref="PairResponse.Read(NSDictionary)"/> parses the error data.
        /// </summary>
        [Fact]
        public void Read_WithError_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Error", "SomeError");

            var value = PairResponse.Read(dict);
            Assert.Equal("SomeError", value.Error);
            Assert.Null(value.EscrowBag);
        }
    }
}
