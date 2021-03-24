﻿// <copyright file="PropertyListProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.PropertyLists;
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
            Assert.Throws<ArgumentNullException>("stream", () => new PropertyListProtocol(null, true));
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.WriteMessageAsync(NSDictionary, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Write_ValidatesArguments_Async()
        {
            var protocol = new PropertyListProtocol(Stream.Null, false);

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
            await using (var protocol = new PropertyListProtocol(stream, false))
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
            await using (var protocol = new PropertyListProtocol(stream, false))
            {
                var dict = await protocol.ReadMessageAsync(default);

                Assert.Single(dict);
                Assert.Equal("QueryType", dict["Request"].ToString());
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
            await using (var protocol = new PropertyListProtocol(Stream.Null, false))
            {
                Assert.Null(await protocol.ReadMessageAsync(default).ConfigureAwait(false));
            }
        }
    }
}