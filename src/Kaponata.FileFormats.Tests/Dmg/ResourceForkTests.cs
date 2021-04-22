// <copyright file="ResourceForkTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="ResourceFork"/> class.
    /// </summary>
    public class ResourceForkTests
    {
        /// <summary>
        /// <see cref="ResourceFork.FromPlist(Dictionary{string, object})"/> validates its arguments.
        /// </summary>
        [Fact]
        public void FromPlist_ValidatesArguments()
        {
            var dict = new Dictionary<string, object>();
            Assert.Throws<ArgumentNullException>(() => ResourceFork.FromPlist(null));
            Assert.Throws<ArgumentException>(() => ResourceFork.FromPlist(dict));
        }
    }
}
