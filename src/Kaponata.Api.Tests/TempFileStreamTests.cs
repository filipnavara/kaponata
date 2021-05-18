// <copyright file="TempFileStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.IO;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="TempFileStream"/> class.
    /// </summary>
    public class TempFileStreamTests
    {
        /// <summary>
        /// The <see cref="TempFileStream"/> creates a new file which initialized, and deletes the file when being disposed of.
        /// </summary>
        [Fact]
        public void Lifecycle()
        {
            var stream = new TempFileStream();
            Assert.True(File.Exists(stream.FileName));

            stream.Dispose();
            Assert.False(File.Exists(stream.FileName));
        }
    }
}
