// <copyright file="RegistryDeveloperDiskStoreTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.FileFormats.Tar;
using Kaponata.iOS;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.Kubernetes.DeveloperDisks;
using Kaponata.Kubernetes.Registry;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperDisks
{
    /// <summary>
    /// Tests the <see cref="RegistryDeveloperDiskStore"/> class.
    /// </summary>
    public class RegistryDeveloperDiskStoreTests
    {
        /// <summary>
        /// The <see cref="RegistryDeveloperDiskStore.RegistryDeveloperDiskStore(ImageRegistryClient)"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistryDeveloperDiskStore(null));
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.AddAsync(DeveloperDisk, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddAsync_ValidatesArguments_Async()
        {
            var store = new RegistryDeveloperDiskStore(Mock.Of<ImageRegistryClient>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.AddAsync(DeveloperDisk, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddAsync_Works_Async()
        {
            var disk = new DeveloperDisk()
            {
                Image = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world!")),
                Signature = new byte[] { 1, 2, 3, 4 },
                Version = new SystemVersion()
                {
                    BuildID = new Guid("5abf1921-e3e3-4bfc-94c5-6c6805f23815"),
                    ProductBuildVersion = new AppleVersion(1, 'A', 1),
                    ProductCopyright = "Quamotion bv",
                    ProductName = "Kaponata",
                    ProductVersion = new Version(1, 0),
                },
                CreationTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero),
            };

            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            registryClient
                .Setup(c => c.PushBlobAsync("devimg", It.IsAny<Stream>(), "sha256:caad51da051d60541b2544a5984bdfc44ebceb894069007ad63b2ec073c911d0", default))
                .Returns(Task.FromResult(new Uri("http://localhost:5000/v2/devimg/blobs/sha256:caad51da051d60541b2544a5984bdfc44ebceb894069007ad63b2ec073c911d0")))
                .Verifiable();

            registryClient
                .Setup(c => c.PushBlobAsync("devimg", It.IsAny<Stream>(), "sha256:30c5c9a3aeb6eb237abf743b7657657580657e3548e82f6d2ce6e6e4660a9878", default))
                .Returns(Task.FromResult(new Uri("http://localhost:5000/v2/devimg/blobs/sha256:caad51da051d60541b2544a5984bdfc44ebceb894069007ad63b2ec073c911d0")))
                .Verifiable();

            registryClient
                .Setup(c => c.PushManifestAsync("devimg", "1.0", It.IsAny<Stream>(), default))
                .Returns(Task.FromResult(new Uri("http://localhost:5000/v2/devimg/manifests/1.0")))
                .Verifiable();

            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            await store.AddAsync(disk, default).ConfigureAwait(false);

            registryClient.Verify();
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.ListAsync(CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_Works_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            registryClient.Setup(r => r.ListTagsAsync("devimg", default)).ReturnsAsync(new List<string>() { "1.0" });
            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            Assert.Equal(new Version(1, 0), Assert.Single(await store.ListAsync(default)));

            registryClient.Verify();
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.GetAsync(Version, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_ValidatesArguments_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.GetAsync(Version, CancellationToken)"/> returns <see langword="null"/>
        /// when the disk does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            registryClient.Setup(r => r.GetManifestAsync("devimg", "1.0", default)).Returns(Task.FromResult<Manifest>(null)).Verifiable();

            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            Assert.Null(await store.GetAsync(new Version(1, 0), default).ConfigureAwait(false));

            registryClient.Verify();
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.GetAsync(Version, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_Works_Async()
        {
            var manifest = new Manifest()
            {
                Layers = new Descriptor[] { new Descriptor() { Digest = "sha256:2" } },
            };

            var version = new SystemVersion()
            {
                ProductVersion = new Version(1, 0),
                ProductBuildVersion = new AppleVersion(1, 'A', 1),
            };

            using MemoryStream stream = new MemoryStream();
            TarWriter writer = new TarWriter(stream);
            await writer.AddFileAsync("DeveloperDiskImage.dmg", new MemoryStream(new byte[] { 1 }), default).ConfigureAwait(false);
            await writer.AddFileAsync("DeveloperDiskImage.dmg.signature", new MemoryStream(new byte[] { 2 }), default).ConfigureAwait(false);
            await writer.AddFileAsync("SystemVersion.plist", new MemoryStream(BinaryPropertyListWriter.WriteToArray(version.ToDictionary())), default).ConfigureAwait(false);
            await writer.WriteTrailerAsync(default);

            stream.Position = 0;

            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            registryClient.Setup(r => r.GetManifestAsync("devimg", "1.0", default)).Returns(Task.FromResult(manifest)).Verifiable();
            registryClient.Setup(r => r.GetBlobAsync("devimg", "sha256:2", default)).ReturnsAsync(stream).Verifiable();

            var store = new RegistryDeveloperDiskStore(registryClient.Object);
            var disk = await store.GetAsync(new Version(1, 0), default).ConfigureAwait(false);

            Assert.Equal(new byte[] { 1 }, ((MemoryStream)disk.Image).ToArray());
            Assert.Equal(new byte[] { 2 }, disk.Signature);
            Assert.Equal(new Version(1, 0), disk.Version.ProductVersion);
            Assert.Equal(new AppleVersion(1, 'A', 1), disk.Version.ProductBuildVersion);

            registryClient.Verify();
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.DeleteAsync(Version, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ValidatesArguments_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            var store = new RegistryDeveloperDiskStore(registryClient.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.DeleteAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.DeleteAsync(Version, CancellationToken)"/> does nothing when the disk
        /// does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_DoesNotExist_Returns_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            registryClient.Setup(r => r.GetManifestAsync("devimg", "1.0", default)).Returns(Task.FromResult<Manifest>(null)).Verifiable();

            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            await store.DeleteAsync(new Version(1, 0), default).ConfigureAwait(false);

            registryClient.Verify();
        }

        /// <summary>
        /// <see cref="RegistryDeveloperDiskStore.DeleteAsync(Version, CancellationToken)"/> correctly deletes the disk.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_Works_Async()
        {
            var registryClient = new Mock<ImageRegistryClient>(MockBehavior.Strict);
            var manifest = new Manifest()
            {
                Config = new Descriptor()
                {
                    Digest = "sha256:1",
                },
                Layers = new Descriptor[]
                {
                     new Descriptor()
                     {
                         Digest = "sha256:2",
                     },
                },
            };

            registryClient.Setup(r => r.GetManifestAsync("devimg", "1.0", default)).Returns(Task.FromResult(manifest));
            registryClient.Setup(r => r.DeleteBlobAsync("devimg", "sha256:1", default)).Returns(Task.CompletedTask).Verifiable();
            registryClient.Setup(r => r.DeleteBlobAsync("devimg", "sha256:2", default)).Returns(Task.CompletedTask).Verifiable();
            registryClient.Setup(r => r.DeleteManifestAsync("devimg", "1.0", default)).Returns(Task.CompletedTask).Verifiable();

            var store = new RegistryDeveloperDiskStore(registryClient.Object);

            await store.DeleteAsync(new Version(1, 0), default).ConfigureAwait(false);

            registryClient.Verify();
        }
    }
}
