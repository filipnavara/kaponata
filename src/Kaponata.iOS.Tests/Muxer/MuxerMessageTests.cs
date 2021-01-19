// <copyright file="MuxerMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Muxer;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerMessage" /> class.
    /// </summary>
    public class MuxerMessageTests
    {
        /// <summary>
        /// The <see cref="MuxerMessage.ReadAny(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void ReadAny_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => MuxerMessage.ReadAny(null));
        }

        /// <summary>
        /// The <see cref="MuxerMessage.ReadAny(NSDictionary)"/> method throws an
        /// <see cref="ArgumentOutOfRangeException"/> when passed an empty dictionary.
        /// </summary>
        [Fact]
        public void ReadAny_EmptyDictionary_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("data", () => MuxerMessage.ReadAny(new NSDictionary()));
        }

        /// <summary>
        /// The <see cref="MuxerMessage.ReadAny(NSDictionary)"/> method throws an
        /// <see cref="ArgumentOutOfRangeException"/> when passed an empty dictionary.
        /// </summary>
        [Fact]
        public void ReadAny_InvalidMessageType_ThrowsException()
        {
            var dict = new NSDictionary();
            dict.Add("MessageType", new NSString(nameof(MuxerMessageType.None)));
            Assert.Throws<ArgumentOutOfRangeException>("data", () => MuxerMessage.ReadAny(new NSDictionary()));
        }

        /// <summary>
        /// The <see cref="MuxerMessage.ReadAny(NSDictionary)"/> correctly parses muxer messages.
        /// </summary>
        [Fact]
        public void ReadAny_ReadMessage()
        {
            Assert.IsType<DeviceAttachedMessage>(MuxerMessage.ReadAny((NSDictionary)PropertyListParser.Parse("Muxer/attached.xml")));
            Assert.IsType<DeviceDetachedMessage>(MuxerMessage.ReadAny((NSDictionary)PropertyListParser.Parse("Muxer/detached.xml")));
            Assert.IsType<DevicePairedMessage>(MuxerMessage.ReadAny((NSDictionary)PropertyListParser.Parse("Muxer/paired.xml")));
            Assert.IsType<ResultMessage>(MuxerMessage.ReadAny((NSDictionary)PropertyListParser.Parse("Muxer/result.xml")));
            Assert.IsType<DeviceListMessage>(MuxerMessage.ReadAny((NSDictionary)PropertyListParser.Parse("Muxer/devicelist.xml")));
        }
    }
}
