// <copyright file="MobileImageMounterClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.MobileImageMounter;
using Kaponata.iOS.PropertyLists;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="MobileImageMounterClient"/> class.
    /// </summary>
    public class MobileImageMounterClientTests
    {
        /// <summary>
        /// The <see cref="MobileImageMounterClient"/> cosntructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClient(null));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClient(null, NullLogger.Instance));
            Assert.Throws<ArgumentNullException>(() => new MobileImageMounterClient(Stream.Null, null));
        }

        /// <summary>
        /// The <see cref="MobileImageMounterClient"/> methods throw when the instance has been disposed of.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Methods_ThrowWhenAsyncDisposed_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            var client = new MobileImageMounterClient(protocol.Object);
            await client.DisposeAsync();

            Assert.True(client.IsDisposed);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.HangupAsync(default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.LookupImageAsync(null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.MountImageAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.UploadImageAsync(null, null, null, default)).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// The <see cref="MobileImageMounterClient"/> methods throw when the instance has been disposed of.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "Testing the .Dispose method")]
        public async Task Methods_ThrowWhenDisposed_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.Dispose()).Verifiable();

            var client = new MobileImageMounterClient(protocol.Object);
            client.Dispose();

            Assert.True(client.IsDisposed);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.HangupAsync(default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.LookupImageAsync(null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.MountImageAsync(null, null, default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.UploadImageAsync(null, null, null, default)).ConfigureAwait(false);

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.LookupImageAsync(string, CancellationToken)"/> validates it arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LookupImageAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.LookupImageAsync(null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.LookupImageAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LookupImageAsync_Works_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LookupImageRequest>(), default))
                .Callback<IPropertyList, CancellationToken>((request, ct) =>
                {
                    var lookupImageRequest = Assert.IsType<LookupImageRequest>(request);
                    Assert.Equal("LookupImage", lookupImageRequest.Command);
                    Assert.Equal("Developer", lookupImageRequest.ImageType);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = new LookupImageResponse();

            protocol
                .Setup(p => p.ReadMessageAsync<LookupImageResponse>(default))
                .ReturnsAsync(response);

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                Assert.Same(response, await client.LookupImageAsync("Developer", default).ConfigureAwait(false));
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.MountImageAsync(byte[], string, CancellationToken)"/> validates it arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MountImageAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.MountImageAsync(null, string.Empty, default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.MountImageAsync(Array.Empty<byte>(), null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.MountImageAsync(byte[], string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MountImageAsync_Works_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MountImageRequest>(), default))
                .Callback<IPropertyList, CancellationToken>((request, ct) =>
                {
                    var mountImageRequest = Assert.IsType<MountImageRequest>(request);
                    Assert.Equal("MountImage", mountImageRequest.Command);
                    Assert.Equal("Developer", mountImageRequest.ImageType);
                    Assert.Equal(new byte[] { 1, 2, 3, 4 }, mountImageRequest.ImageSignature);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = new MobileImageMounterResponse() { Status = MobileImageMounterStatus.Complete };

            protocol
                .Setup(p => p.ReadMessageAsync<MobileImageMounterResponse>(default))
                .ReturnsAsync(response);

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                await client.MountImageAsync(new byte[] { 1, 2, 3, 4 }, "Developer", default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.MountImageAsync(byte[], string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MountImageAsync_HandlesError_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<MountImageRequest>(), default))
                .Callback<IPropertyList, CancellationToken>((request, ct) =>
                {
                    var mountImageRequest = Assert.IsType<MountImageRequest>(request);
                    Assert.Equal("MountImage", mountImageRequest.Command);
                    Assert.Equal("Developer", mountImageRequest.ImageType);
                    Assert.Equal(new byte[] { 1, 2, 3, 4 }, mountImageRequest.ImageSignature);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = new MobileImageMounterResponse() { Status = MobileImageMounterStatus.ReceiveBytesAck };

            protocol
                .Setup(p => p.ReadMessageAsync<MobileImageMounterResponse>(default))
                .ReturnsAsync(response);

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                var ex = await Assert.ThrowsAsync<MobileImageMounterException>(() => client.MountImageAsync(new byte[] { 1, 2, 3, 4 }, "Developer", default));
                Assert.Equal("Invalid image mounter response. Expected Complete but received the ReceiveBytesAck status.", ex.Message);
                Assert.Equal(MobileImageMounterStatus.ReceiveBytesAck, ex.Status);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.HangupAsync(CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task HangupAsync_Works_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<HangupRequest>(), default))
                .Callback<IPropertyList, CancellationToken>((request, ct) =>
                {
                    var hangupRequest = Assert.IsType<HangupRequest>(request);
                    Assert.Equal("Hangup", hangupRequest.Command);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = new HangupResponse() { Status = MobileImageMounterStatus.Complete };

            protocol
                .Setup(p => p.ReadMessageAsync<HangupResponse>(default))
                .ReturnsAsync(response);

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                await client.HangupAsync(default).ConfigureAwait(false);
            }

            protocol.Verify();
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.UploadImageAsync(Stream, string, byte[], CancellationToken)"/> validates it arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UploadImageAsync_ValidatesArguments_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.UploadImageAsync(null, string.Empty, Array.Empty<byte>(), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.UploadImageAsync(Stream.Null, null, Array.Empty<byte>(), default)).ConfigureAwait(false);
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.UploadImageAsync(Stream.Null, string.Empty, null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.UploadImageAsync(Stream, string, byte[], CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UploadImageAsync_Works_Async()
        {
            using (MemoryStream imageStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!")))
            using (MemoryStream protocolStream = new MemoryStream())
            {
                var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
                protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

                protocol
                    .Setup(p => p.WriteMessageAsync(It.IsAny<UploadImageRequest>(), default))
                    .Callback<IPropertyList, CancellationToken>((request, ct) =>
                    {
                        var uploadImageRequest = Assert.IsType<UploadImageRequest>(request);
                        Assert.Equal("ReceiveBytes", uploadImageRequest.Command);
                        Assert.Equal("Developer", uploadImageRequest.ImageType);
                        Assert.Equal(13, uploadImageRequest.ImageSize);
                        Assert.Equal(new byte[] { 1, 2, 3, 4 }, uploadImageRequest.ImageSignature);
                    })
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                Queue<MobileImageMounterResponse> responses = new Queue<MobileImageMounterResponse>(
                    new List<MobileImageMounterResponse>()
                    {
                        new MobileImageMounterResponse() { Status = MobileImageMounterStatus.ReceiveBytesAck },
                        new MobileImageMounterResponse() { Status = MobileImageMounterStatus.Complete },
                    });

                protocol
                    .Setup(p => p.ReadMessageAsync<MobileImageMounterResponse>(default))
                    .ReturnsAsync(responses.Dequeue);

                protocol.Setup(p => p.Stream).Returns(protocolStream);

                await using (var client = new MobileImageMounterClient(protocol.Object))
                {
                    await client.UploadImageAsync(imageStream, "Developer", new byte[] { 1, 2, 3, 4 }, default).ConfigureAwait(false);
                }

                protocol.Verify();

                Assert.Equal("Hello, World!", Encoding.UTF8.GetString(protocolStream.ToArray()));
                Assert.Empty(responses);
            }
        }

        /// <summary>
        /// <see cref="MobileImageMounterClient.UploadImageAsync(Stream, string, byte[], CancellationToken)"/> handles errors correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UploadImageAsync_HandlesError_Async()
        {
            var protocol = new Mock<PropertyListProtocol>(MockBehavior.Strict);
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<UploadImageRequest>(), default))
                .Callback<IPropertyList, CancellationToken>((request, ct) =>
                {
                    var uploadImageRequest = Assert.IsType<UploadImageRequest>(request);
                    Assert.Equal("ReceiveBytes", uploadImageRequest.Command);
                    Assert.Equal("Developer", uploadImageRequest.ImageType);
                    Assert.Equal(0, uploadImageRequest.ImageSize);
                    Assert.Equal(new byte[] { 1, 2, 3, 4 }, uploadImageRequest.ImageSignature);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = new MobileImageMounterResponse();
            response.FromDictionary((NSDictionary)XmlPropertyListParser.Parse(File.ReadAllBytes("MobileImageMounter/mountImageAlreadyMountedResponse.bin")));

            protocol
                .Setup(p => p.ReadMessageAsync<MobileImageMounterResponse>(default))
                .ReturnsAsync(response);

            await using (var client = new MobileImageMounterClient(protocol.Object))
            {
                var exception = await Assert.ThrowsAsync<MobileImageMounterException>(() => client.UploadImageAsync(Stream.Null, "Developer", new byte[] { 1, 2, 3, 4 }, default)).ConfigureAwait(false);
                Assert.Equal("Invalid image mounter response: : ImageMountFailed", exception.Message);
                Assert.Equal(MobileImageMounterError.ImageMountFailed, exception.Error);
                Assert.Null(exception.Status);
                Assert.NotNull(exception.DetailedError);
            }

            protocol.Verify();
        }
    }
}
