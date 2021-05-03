// <copyright file="CatalogThreadTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="CatalogThread"/> class.
    /// </summary>
    public class CatalogThreadTests
    {
        /// <summary>
        /// <see cref="CatalogThread.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            byte[] data = Convert.FromBase64String("AAMAAAAAAAEABQBYAGMAbwBkAGU=");

            CatalogThread thread = new CatalogThread();
            Assert.Equal(20, thread.ReadFrom(data, 0));
            Assert.Equal("Xcode", thread.Name);
            Assert.Equal(new CatalogNodeId(1), thread.ParentId);
            Assert.Equal(CatalogRecordType.FolderThreadRecord, thread.RecordType);
            Assert.Equal(20, thread.Size);
        }

        /// <summary>
        /// <see cref="CatalogThread.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var thread = new CatalogThread();
            Assert.Throws<NotImplementedException>(() => thread.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
