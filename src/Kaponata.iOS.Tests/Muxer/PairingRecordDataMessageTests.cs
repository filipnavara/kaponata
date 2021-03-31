// <copyright file="PairingRecordDataMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Muxer;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="PairingRecordDataMessage"/> class.
    /// </summary>
    public class PairingRecordDataMessageTests
    {
        /// <summary>
        /// <see cref="PairingRecordDataMessage.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => PairingRecordDataMessage.Read(null));
        }

        /// <summary>
        /// <see cref="PairingRecordDataMessage.Read(NSDictionary)"/> correctly deserializes data.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            var data = new byte[] { 1, 2, 3, 4 };
            dict.Add("PairRecordData", data);

            var result = PairingRecordDataMessage.Read(dict);
            Assert.Equal(data, result.PairRecordData);
        }
    }
}
