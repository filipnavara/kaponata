// <copyright file="RegistryDeveloperDiskStore.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.FileFormats.Tar;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.Kubernetes.Registry;
using Nerdbank.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.DeveloperDisks
{
    /// <summary>
    /// Implements the <see cref="DeveloperDiskStore"/> class using a container image registry.
    /// </summary>
    public class RegistryDeveloperDiskStore : DeveloperDiskStore
    {
        private readonly ImageRegistryClient client;
        private readonly string repositoryName = "devimg";

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryDeveloperDiskStore"/> class.
        /// </summary>
        /// <param name="client">
        /// A <see cref="ImageRegistryClient"/> which provides access to the underlying registry.
        /// </param>
        public RegistryDeveloperDiskStore(ImageRegistryClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public override async Task AddAsync(DeveloperDisk disk, CancellationToken cancellationToken)
        {
            if (disk == null)
            {
                throw new ArgumentNullException(nameof(disk));
            }

            var factory = new DeveloperDiskRegistryImageFactory();
            (var manifest, var configStream, var layerStream) = await factory.CreateRegistryImageAsync(disk, cancellationToken).ConfigureAwait(false);

            using (layerStream)
            using (configStream)
            using (Stream manifestStream = new MemoryStream())
            {
                // Send over the base layer
                Descriptor layerDescriptor = await Descriptor.CreateAsync(layerStream, "application/vnd.oci.image.layer.nondistributable.v1.tar", cancellationToken);
                await this.client.PushBlobAsync(this.repositoryName, layerStream, layerDescriptor.Digest, cancellationToken).ConfigureAwait(false);

                // Prepare and send the configuration
                var configDescriptor = await Descriptor.CreateAsync(configStream, "application/vnd.oci.image.config.v1+json", cancellationToken).ConfigureAwait(false);
                await this.client.PushBlobAsync(this.repositoryName, configStream, configDescriptor.Digest, cancellationToken).ConfigureAwait(false);

                // Prepare and send the manifest
                manifestStream.Write(JsonSerializer.SerializeToUtf8Bytes(manifest));

                await this.client.PushManifestAsync(this.repositoryName, disk.Version.ProductVersion.ToString(), manifestStream, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public override async Task<List<Version>> ListAsync(CancellationToken cancellationToken)
        {
            var tags = await this.client.ListTagsAsync(this.repositoryName, cancellationToken).ConfigureAwait(false);

            var versions = new List<Version>();

            foreach (var tag in tags)
            {
                if (Version.TryParse(tag, out Version? version))
                {
                    versions.Add(version);
                }
            }

            return versions;
        }

        /// <inheritdoc/>
        public override async Task<DeveloperDisk?> GetAsync(Version version, CancellationToken cancellationToken)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            var manifest = await this.client.GetManifestAsync(this.repositoryName, version.ToString(), cancellationToken).ConfigureAwait(false);

            if (manifest == null)
            {
                return null;
            }

            // We should perform some sanity checks here.
            // Skip the config for now; although it is stored as a regular blob.
            // var config = await this.client.GetBlobAsync
            using (var layer = await this.client.GetBlobAsync(this.repositoryName, manifest.Layers[0].Digest, cancellationToken).ConfigureAwait(false))
            {
                var disk = new DeveloperDisk();

                TarReader reader = new TarReader(layer);
                TarHeader? header;
                Stream? entryStream;

                while (((header, entryStream) = await reader.ReadAsync(cancellationToken).ConfigureAwait(false)).header != null)
                {
                    var fileName = header!.Value.FileName;

                    switch (fileName)
                    {
                        case "DeveloperDiskImage.dmg":
                            disk.Image = new MemoryStream();
                            disk.CreationTime = header.Value.LastModified;
                            await entryStream!.CopyToAsync(disk.Image, cancellationToken).ConfigureAwait(false);
                            break;

                        case "DeveloperDiskImage.dmg.signature":
                            disk.Signature = new byte[header.Value.FileSize];
                            await entryStream!.ReadBlockAsync(disk.Signature, cancellationToken).ConfigureAwait(false);
                            break;

                        case "SystemVersion.plist":
                            var data = new byte[header.Value.FileSize];
                            await entryStream!.ReadBlockAsync(data, cancellationToken).ConfigureAwait(false);
                            var plist = (NSDictionary)PropertyListParser.Parse(data);

                            disk.Version = new SystemVersion();
                            disk.Version.FromDictionary(plist);
                            break;

                        case "":
                            break;

                        default:
                            throw new InvalidDataException();
                    }
                }

                return disk;
            }
        }

        /// <inheritdoc/>
        public override async Task DeleteAsync(Version version, CancellationToken cancellationToken)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            var manifest = await this.client.GetManifestAsync(this.repositoryName, version.ToString(), cancellationToken).ConfigureAwait(false);

            if (manifest == null)
            {
                // Nothing to do.
                return;
            }

            // Delete the manifest, and then the unused blob objects (configuration + RootFS layers)
            await this.client.DeleteManifestAsync(this.repositoryName, version.ToString(), cancellationToken).ConfigureAwait(false);
            foreach (var layer in manifest.Layers)
            {
                await this.client.DeleteBlobAsync(this.repositoryName, layer.Digest, cancellationToken).ConfigureAwait(false);
            }

            await this.client.DeleteBlobAsync(this.repositoryName, manifest.Config.Digest, cancellationToken).ConfigureAwait(false);
        }
    }
}
