// <copyright file="PortForwardStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="PortForwardStream"/> class.
    /// </summary>
    public class PortForwardStreamTests
    {
        /// <summary>
        /// The <see cref="PortForwardStream"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("webSocket", () => new PortForwardStream(null, NullLogger<PortForwardStream>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new PortForwardStream(Mock.Of<WebSocket>(), null));
        }

        /// <summary>
        /// The <see cref="PortForwardStream"/> class disposes of the underlying socket.
        /// </summary>
        [Fact]
        public void DisposesOfWebSocket()
        {
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.Dispose()).Verifiable();

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
            }

            socket.Verify();
        }

        /// <summary>
        /// The default properties return correct values.
        /// </summary>
        [Fact]
        public void DefaultPropertiesReturnCorrectValeus()
        {
            var socket = Mock.Of<WebSocket>();

            using (var stream = new PortForwardStream(socket, NullLogger<PortForwardStream>.Instance))
            {
                Assert.True(stream.CanRead);
                Assert.False(stream.CanSeek);
                Assert.True(stream.CanWrite);
                Assert.Same(socket, stream.WebSocket);
            }
        }

        /// <summary>
        /// The properties and methods which are not supported throw a <see cref="NotSupportedException"/>.
        /// </summary>
        [Fact]
        public void NotSupportedMethodsAndPropertiesThrow()
        {
            using (var stream = new PortForwardStream(Mock.Of<WebSocket>(), NullLogger<PortForwardStream>.Instance))
            {
                Assert.True(stream.CanRead);
                Assert.False(stream.CanSeek);
                Assert.True(stream.CanWrite);
                Assert.Throws<NotSupportedException>(() => stream.Length);
                Assert.Throws<NotSupportedException>(() => stream.Position);
                Assert.Throws<NotSupportedException>(() => { stream.Position = 1; });
                Assert.Throws<NotSupportedException>(() => stream.Read(Array.Empty<byte>(), 0, 0));
                Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Current));
                Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
                Assert.Throws<NotSupportedException>(() => stream.Write(Array.Empty<byte>(), 0, 0));
            }
        }

        /// <summary>
        /// Calls into <see cref="PortForwardStream.ReadAsync(byte[], int, int, CancellationToken)"/> read data from the
        /// WebSocket. Kubernetes-specific metadata is stripped and the actual payload is copied to the caller.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessage_Async()
        {
            Queue<(bool, byte[])> packets = new Queue<(bool, byte[])>(
                new (bool, byte[])[]
                {
                    // Initialize stream 0 on port 80
                    (true, new byte[] { 0, 0, 80 }),

                    // Initialize stream 1 (error stream) on port 80
                    (true, new byte[] { 1, 0, 80 }),

                    // Partial reads on stream 0, data contains values 1 through 8
                    (false, new byte[] { 0, 1, 2, 3 }),
                    (false, new byte[] { 4, 5, 6, 7 }),
                    (true, new byte[] { 8 }),
                });

            var socket = new Mock<WebSocket>(MockBehavior.Strict);
            socket.Setup(s => s.Dispose()).Verifiable();
            socket
                .Setup(s => s.ReceiveAsync(It.IsAny<Memory<byte>>(), default))
                .Returns<Memory<byte>, CancellationToken>((buffer, ct) =>
                {
                    (var endOfMessage, byte[] data) = packets.Dequeue();
                    data.AsMemory().CopyTo(buffer);
                    ValueWebSocketReceiveResult result =
                    new ValueWebSocketReceiveResult(data.Length, WebSocketMessageType.Binary, endOfMessage);

                    return ValueTask.FromResult(result);
                });

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
                byte[] data = new byte[8];
                Assert.Equal(3, await stream.ReadAsync(data, 0, 8, default).ConfigureAwait(false));
                Assert.Equal(4, await stream.ReadAsync(data, 3, 5, default).ConfigureAwait(false));
                Assert.Equal(1, await stream.ReadAsync(data, 7, 1, default).ConfigureAwait(false));
            }

            socket.Verify();
        }

        /// <summary>
        /// Calls into <see cref="PortForwardStream.ReadAsync(byte[], int, int, CancellationToken)"/> read data from the
        /// WebSocket. Kubernetes-specific metadata is stripped and the actual payload is copied to the caller.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task ReadMessage_HandlesError_Async()
        {
            Queue<(bool, byte[])> packets = new Queue<(bool, byte[])>(
                new (bool, byte[])[]
                {
                    // Initialize stream 0 on port 80
                    (true, new byte[] { 0, 0, 80 }),

                    // Initialize stream 1 (error stream) on port 80
                    (true, new byte[] { 1, 0, 80 }),

                    // An error message
                    (true, new byte[] { 1, (byte)'E', (byte)'R', (byte)'R', (byte)'O', (byte)'R' }),
                });

            var socket = new Mock<WebSocket>(MockBehavior.Strict);
            socket.Setup(s => s.Dispose()).Verifiable();
            socket
                .Setup(s => s.ReceiveAsync(It.IsAny<Memory<byte>>(), default))
                .Returns<Memory<byte>, CancellationToken>((buffer, ct) =>
                {
                    (var endOfMessage, byte[] data) = packets.Dequeue();
                    data.AsMemory().CopyTo(buffer);
                    ValueWebSocketReceiveResult result =
                    new ValueWebSocketReceiveResult(data.Length, WebSocketMessageType.Binary, endOfMessage);

                    return ValueTask.FromResult(result);
                });

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
                byte[] data = new byte[8];
                var exception = await Assert.ThrowsAsync<IOException>(() => stream.ReadAsync(data, 0, 8, default)).ConfigureAwait(false);
                Assert.Equal("ERROR", exception.Message);
            }
        }

        /// <summary>
        /// Calls to <see cref="PortForwardStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/> are handled correctly;
        /// the data is encapsulated in the Kubernetes-specific protocol and forwarded to the remote end.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteAsync_Works_Async()
        {
            byte[] data = new byte[] { 1, 2, 3 };
            byte[] encapsulatedData = new byte[] { 0, 1, 2, 3 };

            var socket = new Mock<WebSocket>(MockBehavior.Strict);
            socket.Setup(s => s.Dispose()).Verifiable();
            socket
                .Setup(s => s.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), WebSocketMessageType.Binary, false, default))
                .Callback<ReadOnlyMemory<byte>, WebSocketMessageType, bool, CancellationToken>(
                    (buffer, messageType, endOfMessage, cancellationToken)
                    =>
                    {
                        Assert.Equal(encapsulatedData, buffer.ToArray());
                    })
                .Returns(ValueTask.CompletedTask);

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
                await stream.WriteAsync(data.AsMemory(), default).ConfigureAwait(false);
            }

            socket.Verify();
        }

        /// <summary>
        /// Calls to <see cref="PortForwardStream.WriteAsync(byte[], int, int, CancellationToken)"/> are handled correctly;
        /// the data is encapsulated in the Kubernetes-specific protocol and forwarded to the remote end.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WriteAsync_Buffer_Works_Async()
        {
            byte[] data = new byte[] { 1, 2, 3 };
            byte[] encapsulatedData = new byte[] { 0, 1, 2, 3 };

            var socket = new Mock<WebSocket>(MockBehavior.Strict);
            socket.Setup(s => s.Dispose()).Verifiable();
            socket
                .Setup(s => s.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), WebSocketMessageType.Binary, false, default))
                .Callback<ReadOnlyMemory<byte>, WebSocketMessageType, bool, CancellationToken>(
                    (buffer, messageType, endOfMessage, cancellationToken)
                    =>
                    {
                        Assert.Equal(encapsulatedData, buffer.ToArray());
                    })
                .Returns(ValueTask.CompletedTask);

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
                await stream.WriteAsync(data, 0, data.Length, default).ConfigureAwait(false);
            }

            socket.Verify();
        }

        /// <summary>
        /// Calls to <see cref="PortForwardStream.Flush"/> are no-ops.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Flush_IsNoOp_Async()
        {
            var socket = new Mock<WebSocket>(MockBehavior.Strict);
            socket.Setup(s => s.Dispose()).Verifiable();

            using (var stream = new PortForwardStream(socket.Object, NullLogger<PortForwardStream>.Instance))
            {
                await stream.FlushAsync().ConfigureAwait(false);
            }

            socket.Verify();
        }
    }
}
