// <copyright file="XipFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Streams;
using Kaponata.FileFormats.Cpio;
using Kaponata.FileFormats.Xip;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.FileFormats.Tests.Xip
{
    /// <summary>
    /// Tests the <see cref="XipFile"/> class.
    /// </summary>
    public class XipFileTests
    {
        /// <summary>
        /// The <see cref="XipFile.XipFile(Stream, Ownership)"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new XipFile(null, Ownership.Dispose));
        }

        /// <summary>
        /// Tests the <see cref="XipFile.Open"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task Open_Works_Async()
        {
            using (var file = File.OpenRead("TestAssets/test.xip"))
            using (var xip = new XipFile(file))
            using (var cpio = xip.Open())
            {
                CpioHeader? header;
                string name;
                Stream stream;

                Collection<string> fileNames = new Collection<string>();

                while (((header, name, stream) = await cpio.ReadAsync(default).ConfigureAwait(false)).name != null)
                {
                    fileNames.Add(name);
                }

                Assert.Collection(
                    fileNames,
                    f => Assert.Equal(".", f),
                    f => Assert.Equal("./dmg", f),
                    f => Assert.Equal("./dmg/System", f),
                    f => Assert.Equal("./dmg/System/Library", f),
                    f => Assert.Equal("./dmg/System/Library/CoreServices", f),
                    f => Assert.Equal("./dmg/System/Library/CoreServices/SystemVersion.plist", f),
                    f => Assert.Equal("./dmg/hello.txt", f));

                Assert.Null(xip.Metadata.FileSystemCompressionFormat);
                Assert.Equal(0x1f8, xip.Metadata.UncompressedSize);
                Assert.Equal(1, xip.Metadata.Version);
            }
        }

        /// <summary>
        /// <see cref="XipFile.Dispose"/> works correctly.
        /// </summary>
        [Fact]
        public void Dispose_Works()
        {
            using (var file = File.OpenRead("TestAssets/test.xip"))
            using (var xip = new XipFile(file, Ownership.Dispose))
            {
                Assert.False(xip.IsDisposed);

                xip.Dispose();

                Assert.True(xip.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => xip.Open());

                Assert.Throws<ObjectDisposedException>(() => file.Length);
            }
        }
    }
}
