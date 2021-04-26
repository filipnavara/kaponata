// <copyright file="XarFileEntryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Xar;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.Xar
{
    /// <summary>
    /// Tests the <see cref="XarFileEntry"/> class.
    /// </summary>
    public class XarFileEntryTests
    {
        /// <summary>
        /// The <see cref="XarFileEntry"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new XarFileEntry(null));
        }
    }
}
