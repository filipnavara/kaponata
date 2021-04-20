// <copyright file="TarWriterTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.FileFormats.Tests.Tar
{
    /// <summary>
    /// Tests the <see cref="TarWriter"/> class.
    /// </summary>
    public class TarWriterTests
    {
        /// <summary>
        /// The <see cref="TarWriter"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new TarWriter(null));
        }

        /// <summary>
        /// <see cref="TarWriter.AddFileAsync(string, Stream, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFileAsync_ValidatesArguments_Async()
        {
            var writer = new TarWriter(Stream.Null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => writer.AddFileAsync(null, Stream.Null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => writer.AddFileAsync(string.Empty, null, default)).ConfigureAwait(false);

            await Assert.ThrowsAsync<ArgumentNullException>(() => writer.AddFileAsync(null, LinuxFileMode.None, DateTimeOffset.Now, Stream.Null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => writer.AddFileAsync(string.Empty, LinuxFileMode.None, DateTimeOffset.Now, null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="TarWriter"/> can create simple tar archives.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSimpleArchive_Async()
        {
            using (MemoryStream tarStream = new MemoryStream())
            using (MemoryStream entryStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!\r\n")))
            {
                var writer = new TarWriter(tarStream);

                await writer.AddFileAsync(
                    "hello.txt",
                    LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IFREG,
                    new DateTimeOffset(2021, 4, 19, 15, 13, 11, TimeSpan.Zero),
                    entryStream,
                    default).ConfigureAwait(false);

                await writer.WriteTrailerAsync(default);

                File.WriteAllBytes("Tar/rootfs.actual.tar", tarStream.ToArray());
                Assert.Equal(File.ReadAllBytes("Tar/rootfs.tar"), tarStream.ToArray());
            }
        }
    }
}
