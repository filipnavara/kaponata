// <copyright file="MuxerProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Muxer;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Muxer
{
    /// <summary>
    /// Tests the <see cref="MuxerProtocol"/> class.
    /// </summary>
    public class MuxerProtocolTests
    {
        /// <summary>
        /// The <see cref="MuxerProtocol"/> class disposes of the underlying stream if it owns the stream.
        /// </summary>
        /// <param name="ownsStream">
        /// A value indicating whether the <see cref="MuxerProtocol"/> owns the stream.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MuxerProtocol_DisposeAsync_DisposesStreamIfNeeded_Async(bool ownsStream)
        {
            var stream = new Mock<Stream>(MockBehavior.Strict);

            if (ownsStream)
            {
                stream.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            }

            var protocol = new MuxerProtocol(stream.Object, ownsStream, NullLogger<MuxerProtocol>.Instance);
            await protocol.DisposeAsync();

            stream.Verify();
        }

        /// <summary>
        /// The <see cref="MuxerProtocol"/> constructor checks for <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void MuxerProtocol_ConstructorValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("stream", () => new MuxerProtocol(null, ownsStream: true, NullLogger<MuxerProtocol>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new MuxerProtocol(Stream.Null, ownsStream: true, null));
        }

        /// <summary>
        /// The <see cref="MuxerProtocol.WriteMessageAsync(MuxerMessage, CancellationToken)"/> method checks for <see langword="null"/>
        /// values.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteMessageAsync_ValidatesNullArgument_Async()
        {
            await using (var protocol = new MuxerProtocol(Stream.Null, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("message", () => protocol.WriteMessageAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The <see cref="MuxerProtocol.WriteMessageAsync(MuxerMessage, CancellationToken)"/> method correctly serializes simple messages.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteMessageAsync_SerializesCorrectly_Async()
        {
            await using (MemoryStream stream = new MemoryStream())
            await using (var protocol = new MuxerProtocol(stream, ownsStream: true, NullLogger<MuxerProtocol>.Instance))
            {
                await protocol.WriteMessageAsync(
                    new RequestMessage()
                    {
                        MessageType = MuxerMessageType.ListDevices,
                        BundleID = "com.apple.iTunes",
                        ClientVersionString = "usbmuxd-374.70",
                        ProgName = "iTunes",
                    },
                    default).ConfigureAwait(false);

                var actual = stream.ToArray();
                var expected = File.ReadAllBytes("Muxer/list-request.bin");

                Assert.Equal(
                    expected,
                    actual);
            }
        }
    }
}
