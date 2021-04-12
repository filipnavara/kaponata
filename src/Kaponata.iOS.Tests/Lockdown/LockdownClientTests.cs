// <copyright file="LockdownClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownClient"/> class.
    /// </summary>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// The <see cref="LockdownClient"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new LockdownClient((Stream)null, NullLogger<LockdownClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Stream.Null, null));

            Assert.Throws<ArgumentNullException>(() => new LockdownClient((LockdownProtocol)null, NullLogger<LockdownClient>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LockdownClient(Mock.Of<LockdownProtocol>(), null));
        }
    }
}
