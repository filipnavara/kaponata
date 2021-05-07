// <copyright file="DeveloperDiskRegistryImageFactoryTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.FileFormats;
using Kaponata.FileFormats.Tar;
using Kaponata.iOS;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.Kubernetes.DeveloperDisks;
using Kaponata.Kubernetes.Registry;
using Nerdbank.Streams;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperDisks
{
    /// <summary>
    /// Tests the <see cref="DeveloperDiskRegistryImageFactory"/> class.
    /// </summary>
    public class DeveloperDiskRegistryImageFactoryTests
    {
        /// <summary>
        /// <see cref="DeveloperDiskRegistryImageFactory.CreateRegistryImageAsync(DeveloperDisk, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Create_ValidatesArguments_Async()
        {
            var factory = new DeveloperDiskRegistryImageFactory();
            await Assert.ThrowsAsync<ArgumentNullException>(() => factory.CreateRegistryImageAsync(null, default));
        }

        /// <summary>
        /// <see cref="DeveloperDiskRegistryImageFactory.CreateRegistryImageAsync(DeveloperDisk, CancellationToken)"/> returns a proper manifest,
        /// image configuration and RootFS layer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Create_Works_Async()
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

            var factory = new DeveloperDiskRegistryImageFactory();
            (var manifest, var configStream, var layerStream) = await factory.CreateRegistryImageAsync(disk, default).ConfigureAwait(false);

            Assert.NotNull(manifest);
            Assert.NotNull(configStream);
            Assert.Equal(0, configStream.Position);
            Assert.NotEqual(0, configStream.Length);

            Assert.NotNull(layerStream);
            Assert.Equal(0, layerStream.Position);
            Assert.NotEqual(0, layerStream.Length);

            var config = await JsonSerializer.DeserializeAsync<Image>(configStream, default).ConfigureAwait(false);

            // The RootFS is a Tar archive which contains the developer disk image, signature and a copy of the SystemVersion.plist file
            var reader = new TarReader(layerStream);
            (TarHeader? header, Stream entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Equal("SystemVersion.plist", header.Value.FileName);
            Assert.Equal(477u, header.Value.FileSize);
            Assert.Equal(LinuxFileMode.S_IFREG | LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IRUSR, header.Value.FileMode);
            Assert.Equal(string.Empty, header.Value.UserName);
            Assert.Equal(string.Empty, header.Value.GroupName);

            var systemVersion = new SystemVersion();
            systemVersion.FromDictionary((NSDictionary)PropertyListParser.Parse(entryStream));
            Assert.Equal(new AppleVersion(1, 'A', 1), systemVersion.ProductBuildVersion);
            Assert.Equal(new Version(1, 0), systemVersion.ProductVersion);

            (header, entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Equal("DeveloperDiskImage.dmg", header.Value.FileName);
            Assert.Equal(13u, header.Value.FileSize);
            Assert.Equal(LinuxFileMode.S_IFREG | LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IRUSR, header.Value.FileMode);
            Assert.Equal(string.Empty, header.Value.UserName);
            Assert.Equal(string.Empty, header.Value.GroupName);
            byte[] buffer = new byte[header.Value.FileSize];
            await entryStream.ReadBlockOrThrowAsync(buffer).ConfigureAwait(false);
            Assert.Equal("Hello, world!", Encoding.UTF8.GetString(buffer));

            (header, entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Equal("DeveloperDiskImage.dmg.signature", header.Value.FileName);
            Assert.Equal(4u, header.Value.FileSize);
            Assert.Equal(LinuxFileMode.S_IFREG | LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IRUSR, header.Value.FileMode);
            Assert.Equal(string.Empty, header.Value.UserName);
            Assert.Equal(string.Empty, header.Value.GroupName);
            await entryStream.ReadBlockOrThrowAsync(buffer.AsMemory(0, 4)).ConfigureAwait(false);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, buffer.AsSpan(0, 4).ToArray());

            (header, entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Equal(string.Empty, header.Value.FileName);

            (header, entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Equal(string.Empty, header.Value.FileName);

            (header, entryStream) = await reader.ReadAsync(default).ConfigureAwait(false);
            Assert.Null(header);

            // The configuration file
            Assert.Equal("arm64", config.Architecture);
            Assert.Null(config.Author);
            Assert.Null(config.Config);
            Assert.Null(config.Created);
            Assert.Null(config.History);
            Assert.Equal("ios", config.OS);
            Assert.Equal("layers", config.RootFS.Type);
            Assert.Collection(
                config.RootFS.DiffIDs,
                i => Assert.Equal("sha256:caad51da051d60541b2544a5984bdfc44ebceb894069007ad63b2ec073c911d0", i));

            // The manifest
            Assert.Collection(
                manifest.Annotations,
                a =>
                {
                    Assert.Equal("BuildID", a.Key);
                    Assert.Equal("5abf1921-e3e3-4bfc-94c5-6c6805f23815", a.Value);
                },
                a =>
                {
                    Assert.Equal("ProductBuildVersion", a.Key);
                    Assert.Equal("1A1", a.Value);
                },
                a =>
                {
                    Assert.Equal("ProductName", a.Key);
                    Assert.Equal("Kaponata", a.Value);
                },
                a =>
                {
                    Assert.Equal("ProductVersion", a.Key);
                    Assert.Equal("1.0", a.Value);
                },
                a =>
                {
                    Assert.Equal("DeveloperDisk.dmg.signature", a.Key);
                    Assert.Equal("AQIDBA==", a.Value);
                });
            Assert.Null(manifest.Config.Annotations);
            Assert.Equal("sha256:30c5c9a3aeb6eb237abf743b7657657580657e3548e82f6d2ce6e6e4660a9878", manifest.Config.Digest);
            Assert.Equal("application/vnd.oci.image.config.v1+json", manifest.Config.MediaType);
            Assert.Null(manifest.Config.Platform);
            Assert.Equal(149, manifest.Config.Size);
            Assert.Null(manifest.Config.URLs);
        }
    }
}
