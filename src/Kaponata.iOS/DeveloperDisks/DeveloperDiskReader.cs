// <copyright file="DeveloperDiskReader.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using DiscUtils;
using DiscUtils.Dmg;
using DiscUtils.HfsPlus;
using DiscUtils.Streams;
using System;
using System.IO;

namespace Kaponata.iOS.DeveloperDisks
{
    /// <summary>
    /// Parses the content of a developer disk image file, and verifies whether the file is a valid iOS developer
    /// disk image.
    /// </summary>
    public static class DeveloperDiskReader
    {
        /// <summary>
        /// The path to the file which contains version information.
        /// </summary>
        private const string SystemVersionPath = @"System\Library\CoreServices\SystemVersion.plist";

        /// <summary>
        /// Gets a <see cref="SystemVersion"/> object with additional information about the developer
        /// disk image.
        /// </summary>
        /// <param name="developerDiskImageStream">
        /// A <see cref="Stream"/> which represents the developer disk image.
        /// </param>
        /// <returns>
        /// A <see cref="SystemVersion"/> object with additional information about the developer
        /// disk image.
        /// </returns>
        public static (SystemVersion, DateTimeOffset) GetVersionInformation(Stream developerDiskImageStream)
        {
            using (var disk = new Disk(developerDiskImageStream, Ownership.None))
            {
                // Find the first (and supposedly, only, HFS partition)
                var volumes = VolumeManager.GetPhysicalVolumes(disk);

                if (volumes.Length != 1)
                {
                    throw new InvalidDataException($"The developer disk should contain exactly one volume");
                }

                using (var volumeStream = volumes[0].Open())
                using (var hfs = new HfsPlusFileSystem(volumeStream))
                {
                    if (hfs.FileExists(SystemVersionPath))
                    {
                        using (Stream systemVersionStream = hfs.OpenFile(SystemVersionPath, FileMode.Open, FileAccess.Read))
                        {
                            var dict = (NSDictionary)PropertyListParser.Parse(systemVersionStream);
                            SystemVersion plist = new SystemVersion();
                            plist.FromDictionary(dict);

                            if (plist.ProductName != "iPhone OS")
                            {
                                throw new InvalidDataException("The developer disk does not target iOS");
                            }

                            return (plist, hfs.Root.CreationTimeUtc);
                        }
                    }
                }

                throw new InvalidDataException($"The file does not contain any HFS+ parition. Is it a valid developer disk image?");
            }
        }
    }
}
