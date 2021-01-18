// <copyright file="RequestMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Muxer;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="RequestMessage"/> class.
    /// </summary>
    public class RequestMessageTests
    {
        /// <summary>
        /// Tests the <see cref="RequestMessage.ToPropertyList"/> method.
        /// </summary>
        [Fact]
        public void ToPropertyListTest()
        {
            var message = new RequestMessage();
            var dictionary = message.ToPropertyList();

            Assert.Equal(4, dictionary.Count);
            Assert.Equal(new NSString(message.BundleID), dictionary.Get("BundleID"));
            Assert.Equal(new NSString(message.ClientVersionString), dictionary.Get("ClientVersionString"));
            Assert.Equal(new NSString(message.MessageType.ToString()), dictionary.Get("MessageType"));
            Assert.Equal(new NSString(message.ProgName), dictionary.Get("ProgName"));
        }
    }
}
