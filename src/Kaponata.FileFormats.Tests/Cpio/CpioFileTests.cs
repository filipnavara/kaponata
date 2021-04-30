// <copyright file="CpioFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Cpio;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.FileFormats.Tests.Cpio
{
    /// <summary>
    /// Tests the <see cref="CpioFile"/> class.
    /// </summary>
    public class CpioFileTests
    {
        /// <summary>
        /// The <see cref="CpioFile"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new CpioFile(null, true));
        }

        /// <summary>
        /// <see cref="CpioFile.ReadAsync(CancellationToken)"/> throws when invalid data is encountered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadFile_NotAscii_Throws_Async()
        {
            using (MemoryStream stream = new MemoryStream(new byte[CpioHeader.Size]))
            using (CpioFile file = new CpioFile(stream, leaveOpen: true))
            {
                await Assert.ThrowsAsync<FormatException>(() => file.ReadAsync(default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="CpioFile.ReadAsync(CancellationToken)"/> throws when invalid data is encountered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadFile_InvalidHeader_Throws_Async()
        {
            byte[] data = new byte[CpioHeader.Size];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)'0';
            }

            using (MemoryStream stream = new MemoryStream(data))
            using (CpioFile file = new CpioFile(stream, leaveOpen: true))
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => file.ReadAsync(default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="CpioFile.ReadAsync(CancellationToken)"/> throws when insufficient data is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadFile_EmptyStream_Throws_Async()
        {
            using (CpioFile file = new CpioFile(Stream.Null, leaveOpen: true))
            {
                await Assert.ThrowsAsync<EndOfStreamException>(() => file.ReadAsync(default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Tests the <see cref="CpioFile.ReadAsync(CancellationToken)"/> method, looping over an entire CPIO archive.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadFileTest_Async()
        {
            // The CPIO file contains ASCII data only. This collection tracks the content of the current entries.
            Collection<string> entries = new Collection<string>();
            Collection<string> entryNames = new Collection<string>();

            using (Stream stream = File.OpenRead("TestAssets/test.cpio"))
            using (CpioFile file = new CpioFile(stream, leaveOpen: true))
            {
                CpioHeader? header;
                string name;
                Stream childStream;

                while (((header, name, childStream) = await file.ReadAsync(default).ConfigureAwait(false)).header != null)
                {
                    using (StreamReader reader = new StreamReader(childStream))
                    {
                        entries.Add(await reader.ReadToEndAsync().ConfigureAwait(false));
                        entryNames.Add(name);
                    }
                }
            }

            Assert.Collection(
                entries,
                e => Assert.Equal("Hello, World!\n", e));

            Assert.Collection(
                entryNames,
                n => Assert.Equal("dmg/hello.txt", n));
        }

        /// <summary>
        /// <see cref="CpioFile.Dispose"/> disposes of the inner stream when required.
        /// </summary>
        [Fact]
        public void DisposesOfOwnedStream()
        {
            Stream stream = new MemoryStream();

            using (var file = new CpioFile(stream, leaveOpen: false))
            {
                // Do nothing
            }

            Assert.Throws<ObjectDisposedException>(() => stream.Length);
        }
    }
}
