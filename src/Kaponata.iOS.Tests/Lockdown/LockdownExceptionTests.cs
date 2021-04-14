// <copyright file="LockdownExceptionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownException"/> class.
    /// </summary>
    public class LockdownExceptionTests
    {
        /// <summary>
        /// Tests the <see cref="LockdownException"/> constructor.
        /// </summary>
        [Fact]
        public void Constructor_Works()
        {
            var ex = new LockdownException();
            Assert.Equal("An unknown lockdown error occurred", ex.Message);

            ex = new LockdownException("Hello");
            Assert.Equal("Hello", ex.Message);
        }
    }
}
