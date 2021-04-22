// <copyright file="GenericResourceTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="GenericResource"/> class.
    /// </summary>
    public class GenericResourceTests
    {
        /// <summary>
        /// The <see cref="GenericResource"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericResource(string.Empty, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GenericResource(string.Empty, new Dictionary<string, object>()));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new GenericResource(
                    string.Empty,
                    new Dictionary<string, object>()
                    {
                        { "Name", "a" },
                    }));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new GenericResource(
                    string.Empty,
                    new Dictionary<string, object>()
                    {
                        { "Name", "a" },
                        { "Data", 1 },
                    }));
        }

        /// <summary>
        /// The <see cref="GenericResource"/> properties work.
        /// </summary>
        [Fact]
        public void Properties_Work()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Name", "custom" },
                { "Data", new byte[] { 1, 2, 3, 4 } },
            };

            var resource = new GenericResource("custom", dict);

            Assert.Equal("custom", resource.Type);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, resource.Data);
        }
    }
}
