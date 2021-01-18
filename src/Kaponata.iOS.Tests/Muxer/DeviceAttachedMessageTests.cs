﻿// <copyright file="DeviceAttachedMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Muxer;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="DeviceAttachedMessage"/> class.
    /// </summary>
    public class DeviceAttachedMessageTests
    {
        /// <summary>
        /// The <see cref="DeviceAttachedMessage.Read(NSDictionary)"/> method throws an
        /// <see cref="ArgumentNullException"/> when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_NullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>("data", () => DeviceAttachedMessage.Read(null));
        }
    }
}
