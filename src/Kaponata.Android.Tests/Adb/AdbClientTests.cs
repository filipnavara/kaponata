// <copyright file="AdbClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> class.
    /// </summary>
    public class AdbClientTests
    {
        /// <summary>
        /// The <see cref="AdbClient"/> constructors validate the argments being passed.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("logger", () => new AdbClient(null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new AdbClient(NullLogger<AdbClient>.Instance, null));

            Assert.Throws<ArgumentNullException>("logger", () => new AdbClient(null, NullLoggerFactory.Instance, new AdbSocketLocator()));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new AdbClient(NullLogger<AdbClient>.Instance, null, new AdbSocketLocator()));
            Assert.Throws<ArgumentNullException>("socketLocator", () => new AdbClient(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance, null));
        }

        /// <summary>
        /// <see cref="AdbClient.TryConnectToAdbAsync(System.Threading.CancellationToken)"/> returns <see langword="null"/> when the connection is <see langword="null"/>.
        /// <see cref="AdbSocketLocator.GetAdbSocket"/> returns <see langword="null"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryConnectToAdbAsync_ReturnsNull_Async()
        {
            var locator = new Mock<AdbSocketLocator>();
            locator.Setup(l => l.ConnectToAdbAsync(default)).ReturnsAsync((Stream)null);

            var client = new AdbClient(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance, locator.Object);
            Assert.Null(await client.TryConnectToAdbAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="AdbClient.TryConnectToAdbAsync(System.Threading.CancellationToken)"/> returns the <see cref="AdbProtocol"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task TryConnectToAdbAsync_ReturnsProtocol_Async()
        {
            using var memoryStream = new MemoryStream(new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' });
            var locator = new Mock<AdbSocketLocator>();
            locator.Setup(l => l.ConnectToAdbAsync(default)).ReturnsAsync(memoryStream);

            var client = new AdbClient(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance, locator.Object);

            var protocol = await client.TryConnectToAdbAsync(default).ConfigureAwait(false);
            var message = await protocol.ReadStringAsync(4, default).ConfigureAwait(false);
            Assert.Equal("test", message);
        }

        /// <summary>
        /// The <see cref="AdbClient.EnsureDevice(DeviceData)"/> throws when an invalid device is passed.
        /// </summary>
        [Fact]
        public void EnsureDevice_ThrowsOnInvalidDevice()
        {
            var client = new AdbClient(NullLogger<AdbClient>.Instance, NullLoggerFactory.Instance);

            Assert.Throws<ArgumentNullException>("device", () => client.EnsureDevice(null));
            Assert.Throws<ArgumentOutOfRangeException>("device", () => client.EnsureDevice(new DeviceData()));
            Assert.Throws<ArgumentOutOfRangeException>("device", () => client.EnsureDevice(new DeviceData() { Serial = null }));
            Assert.Throws<ArgumentOutOfRangeException>("device", () => client.EnsureDevice(new DeviceData() { Serial = string.Empty }));

            client.EnsureDevice(new DeviceData() { Serial = "123" });
        }

        /// <summary>
        /// A <see cref="AdbClient"/> object can be sourced from a dependency injection container, and uses a <see cref="AdbSocketLocator"/>
        /// configured in that DI container if one is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AdbClient_WorksWithDependencyInjection_Async()
        {
            var locator = new Mock<AdbSocketLocator>();
            var stream = Mock.Of<Stream>();

            locator
                .Setup(l => l.ConnectToAdbAsync(default))
                .ReturnsAsync(stream);

            var services =
                new ServiceCollection()
                .AddScoped<AdbClient>()
                .AddScoped((p) => locator.Object)
                .AddLogging()
                .BuildServiceProvider();

            var client = services.GetRequiredService<AdbClient>();
            await using (var protocol = await client.TryConnectToAdbAsync(default))
            {
                Assert.Same(stream, protocol.Stream);
            }
        }
    }
}
