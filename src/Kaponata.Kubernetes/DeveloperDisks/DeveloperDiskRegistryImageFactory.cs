// <copyright file="DeveloperDiskRegistryImageFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats;
using Kaponata.FileFormats.Tar;
using Kaponata.iOS.DeveloperDisks;
using Kaponata.Kubernetes.Registry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.DeveloperDisks
{
    /// <summary>
    /// Provides methods for creating container images which represent developer disks.
    /// </summary>
    public class DeveloperDiskRegistryImageFactory
    {
        /// <summary>
        /// Asynchronously creates a registry image which contains a <see cref="DeveloperDisk"/>.
        /// </summary>
        /// <param name="developerDisk">
        /// The <see cref="DeveloperDisk"/> for which to create the image.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and, when completed, returns the image manifest,
        /// image configuration and RootFS layer for the registry image containing the devleoper disk image.
        /// </returns>
        public async Task<(Manifest manifest, Stream configStream, Stream layer)> CreateRegistryImageAsync(DeveloperDisk developerDisk, CancellationToken cancellationToken)
        {
            if (developerDisk == null)
            {
                throw new ArgumentNullException(nameof(developerDisk));
            }

            var layer = await CreateLayerAsync(developerDisk, cancellationToken);
            var layerDescriptor = await Descriptor.CreateAsync(layer, "application/vnd.oci.image.layer.nondistributable.v1.tar", cancellationToken).ConfigureAwait(false);

            var config = CreateConfig(layerDescriptor.Digest);
            Stream configStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(configStream, config, cancellationToken: cancellationToken).ConfigureAwait(false);
            configStream.Seek(0, SeekOrigin.Begin);

            var configDescriptor = await Descriptor.CreateAsync(configStream, "application/vnd.oci.image.config.v1+json", cancellationToken).ConfigureAwait(false);
            var manifest = CreateManifest(configDescriptor, layerDescriptor, developerDisk);

            return (manifest, configStream, layer);
        }

        private static async Task<Stream> CreateLayerAsync(DeveloperDisk developerDisk, CancellationToken cancellationToken)
        {
            MemoryStream stream = new MemoryStream();
            var writer = new TarWriter(stream);

            var versionBytes = Encoding.UTF8.GetBytes(developerDisk.Version.ToDictionary().ToXmlPropertyList());

            var fileMode = LinuxFileMode.S_IFREG | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IROTH;

            await writer.AddFileAsync("SystemVersion.plist", fileMode, developerDisk.CreationTime, new MemoryStream(versionBytes), cancellationToken).ConfigureAwait(false);
            await writer.AddFileAsync("DeveloperDiskImage.dmg", fileMode, developerDisk.CreationTime, developerDisk.Image, cancellationToken).ConfigureAwait(false);
            await writer.AddFileAsync("DeveloperDiskImage.dmg.signature", fileMode, developerDisk.CreationTime, new MemoryStream(developerDisk.Signature), cancellationToken).ConfigureAwait(false);
            await writer.WriteTrailerAsync(cancellationToken).ConfigureAwait(false);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private static Image CreateConfig(string layerDiffId)
        {
            return new Image()
            {
                Architecture = Annotations.Architecture.Arm64,
                OS = Annotations.OperatingSystem.iOS,
                RootFS = new RootFS()
                {
                    Type = "layers",
                    DiffIDs = new string[]
                    {
                        /* digest over the layer's uncompressed tar archive */
                        layerDiffId,
                    },
                },
            };
        }

        private static Manifest CreateManifest(Descriptor configDescriptor, Descriptor layerDescriptor, DeveloperDisk developerDisk)
        {
            return new Manifest()
            {
                SchemaVersion = 2,
                Config = configDescriptor,
                Layers = new Descriptor[]
                {
                    layerDescriptor,
                },
                Annotations = new Dictionary<string, string>()
                {
                    { nameof(SystemVersion.BuildID), developerDisk.Version.BuildID!.ToString() },
                    { nameof(SystemVersion.ProductBuildVersion), developerDisk.Version.ProductBuildVersion!.ToString() },
                    { nameof(SystemVersion.ProductName), developerDisk.Version.ProductName! },
                    { nameof(SystemVersion.ProductVersion), developerDisk.Version.ProductVersion!.ToString() },
                    { "DeveloperDisk.dmg.signature", Convert.ToBase64String(developerDisk.Signature) },
                },
            };
        }
    }
}
