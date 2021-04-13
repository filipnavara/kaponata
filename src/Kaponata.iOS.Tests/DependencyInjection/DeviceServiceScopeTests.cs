// <copyright file="DeviceServiceScopeTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Lockdown;
using Kaponata.iOS.Muxer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.DependencyInjection
{
    /// <summary>
    /// Tests the <see cref="DeviceServiceScope{T}"/> class.
    /// </summary>
    public class DeviceServiceScopeTests
    {
        /// <summary>
        /// The <see cref="DeviceServiceScope{T}"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeviceServiceScope<LockdownClient>(null, new MuxerDevice(), Mock.Of<LockdownClient>()));
            Assert.Throws<ArgumentNullException>(() => new DeviceServiceScope<LockdownClient>(Mock.Of<IServiceScope>(), null, Mock.Of<LockdownClient>()));
            Assert.Throws<ArgumentNullException>(() => new DeviceServiceScope<LockdownClient>(Mock.Of<IServiceScope>(), new MuxerDevice(), null));
        }
    }
}
