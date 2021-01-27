// <copyright file="PacketHeaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.ScrCpy;
using System.IO;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="PacketHeader"/> class.
    /// </summary>
    public class PacketHeaderTests
    {
        /// <summary>
        /// The <see cref="PacketHeader.Read(System.Span{byte})"/> method creates a <see cref="PacketHeader"/> struct from the data.
        /// </summary>
        [Fact]
        public void Read_ReadsHeader()
        {
            var packetHeaderData = File.ReadAllBytes("ScrCpy/packet_header.bin");
            var packetHeader = PacketHeader.Read(packetHeaderData);

            Assert.Equal<uint>(22, packetHeader.PacketLength);
            Assert.Equal<ulong>(18446744073709551615, packetHeader.PacketTimeStamp);
        }
    }
}
