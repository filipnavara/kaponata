// <copyright file="SymlinkTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using DiscUtils.Streams;
using Moq;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="Symlink"/> class.
    /// </summary>
    public class SymlinkTests
    {
        /// <summary>
        /// Tests the <see cref="Symlink.TargetPath"/> property.
        /// </summary>
        [Fact]
        public void TargetPath_Works()
        {
            var content = Encoding.UTF8.GetBytes("/var/lib/discutils.so");
            var buffer = new SparseMemoryBuffer(0x100);
            buffer.Write(0, content, 0, content.Length);

            var symlinkMock = new Mock<Symlink>(MockBehavior.Strict, new Context(), new CatalogNodeId(0), new CatalogFileInfo());
            symlinkMock.Setup(s => s.FileContent).Returns(buffer);

            var symlink = symlinkMock.Object;

            Assert.Equal(@"\var\lib\discutils.so", symlink.TargetPath);

            // Calling .TargetPath twice will excercise the caching code path.
            Assert.Equal(@"\var\lib\discutils.so", symlink.TargetPath);
        }
    }
}
