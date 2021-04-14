// <copyright file="PropertyListProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Divergic.Logging.Xunit;
using Kaponata.iOS.PropertyLists;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.PropertyLists
{
    /// <summary>
    /// Tests the <see cref="PropertyListProtocol"/> class.
    /// </summary>
    public class PropertyListProtocolTests
    {
        /// <summary>
        /// The <see cref="PropertyListProtocol"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("stream", () => new PropertyListProtocol(null, true, NullLogger<PropertyListProtocol>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new PropertyListProtocol(Stream.Null, true, null));
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.WriteMessageAsync(NSDictionary, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Write_ValidatesArguments_Async()
        {
            var protocol = new PropertyListProtocol(Stream.Null, false, NullLogger<PropertyListProtocol>.Instance);

            await Assert.ThrowsAsync<ArgumentNullException>(() => protocol.WriteMessageAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.WriteMessageAsync(NSDictionary, CancellationToken)"/> correctly serializes the
        /// underlying message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Write_Works_Async()
        {
            await using (MemoryStream stream = new MemoryStream())
            await using (var protocol = new PropertyListProtocol(stream, false, NullLogger<PropertyListProtocol>.Instance))
            {
                var dict = new NSDictionary();
                dict.Add("Request", "QueryType");
                await protocol.WriteMessageAsync(dict, default).ConfigureAwait(false);

                var data = stream.ToArray();
                Assert.Equal(File.ReadAllBytes("PropertyLists/message.bin"), data);
            }
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.ReadMessageAsync(CancellationToken)"/> correctly deserializes the
        /// underlying message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_Works_Async()
        {
            await using (Stream stream = File.OpenRead("PropertyLists/message.bin"))
            await using (var protocol = new PropertyListProtocol(stream, false, NullLogger<PropertyListProtocol>.Instance))
            {
                var dict = await protocol.ReadMessageAsync(default);

                Assert.Single(dict);
                Assert.Equal("QueryType", dict["Request"].ToString());
            }
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.ReadMessageAsync(CancellationToken)"/> returns <see langword="null"/>
        /// if the stream was unexpectedly closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_PartialRead_ReturnsNull_Async()
        {
            await using (var stream = new MemoryStream(File.ReadAllBytes("PropertyLists/message.bin")))
            await using (var protocol = new PropertyListProtocol(stream, false, NullLogger<PropertyListProtocol>.Instance))
            {
                // Let's pretend the last 4 bytes are missing.
                stream.SetLength(stream.Length - 4);

                Assert.Null(await protocol.ReadMessageAsync(default));
            }
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.ReadMessageAsync(CancellationToken)"/> returns <see langword="null"/>
        /// when the end of stream is reached.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_EndOfStream_ReturnsNull_Async()
        {
            await using (var protocol = new PropertyListProtocol(Stream.Null, false, NullLogger<PropertyListProtocol>.Instance))
            {
                Assert.Null(await protocol.ReadMessageAsync(default).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.WriteMessageAsync(NSDictionary, CancellationToken)"/> properly logs data sent over the wire.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteMessageAsync_Logs_Async()
        {
            using var logger = new CacheLogger();
            await using var protocol = new PropertyListProtocol(Stream.Null, false, logger);

            var dict = new NSDictionary();
            dict.Add("Foo", "Bar");

            await protocol.WriteMessageAsync(dict, default).ConfigureAwait(false);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(
                "Sending data:\n<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n<plist version=\"1.0\">\n<dict>\n	<key>Foo</key>\n	<string>Bar</string>\n</dict>\n</plist>\n",
                entry.Message,
                ignoreLineEndingDifferences: true,
                ignoreWhiteSpaceDifferences: true);
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.ReadMessageAsync(CancellationToken)"/> property logs data sent over the wire.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadMessageAsync_Logs_Async()
        {
            using var logger = new CacheLogger();
            await using var stream = File.OpenRead("PropertyLists/message.bin");
            await using var protocol = new PropertyListProtocol(stream, false, logger);

            await protocol.ReadMessageAsync(default).ConfigureAwait(false);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(
                "Recieving data:\n<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n<plist version=\"1.0\">\n<dict>\n	<key>Request</key>\n	<string>QueryType</string>\n</dict>\n</plist>\n",
                entry.Message,
                ignoreLineEndingDifferences: true,
                ignoreWhiteSpaceDifferences: true);
        }
    }
}
