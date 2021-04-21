// <copyright file="DmgTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils;
using DiscUtils.Dmg;
using DiscUtils.Fat;
using DiscUtils.Setup;
using DiscUtils.Streams;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// High-level integration tests for the <see cref="Disk"/> class. Uses dmg files which have been
    /// generated using <c>hdiutil</c>.
    /// </summary>
    public class DmgTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DmgTests"/> class.
        /// </summary>
        public DmgTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            SetupHelper.RegisterAssembly(typeof(FatFileSystem).Assembly);
        }

        /// <summary>
        /// Lists all <c>.dmg</c> files which contain a partition with a FAT32 filesystem.
        /// </summary>
        /// <returns>
        /// All <c>.dmg</c> files which contain a partition with a FAT32 filesystem.
        /// </returns>
        public static IEnumerable<object[]> GetDmgFiles()
        {
            foreach (var file in Directory.GetFiles("TestAssets", "*-fat32.dmg"))
            {
                if (Path.GetFileName(file).StartsWith("univ")
                    || Path.GetFileName(file).StartsWith("udrw")
                    || Path.GetFileName(file).StartsWith("udif"))
                {
                    continue;
                }

                yield return new object[] { file };
            }
        }

        /// <summary>
        /// Reads an individual file from a FAT32 filesystem on a partition in a DMG file.
        /// </summary>
        /// <param name="path">
        /// The path to the DMG file.
        /// </param>
        [Theory]
        [MemberData(nameof(GetDmgFiles))]
        public void ReadFatFilesystemTest(string path)
        {
            using (Stream developerDiskImageStream = File.OpenRead(path))
            using (var disk = new Disk(developerDiskImageStream, Ownership.None))
            {
                // Find the first (and supposedly, only, FAT partition)
                var volumes = VolumeManager.GetPhysicalVolumes(disk);
                foreach (var volume in volumes)
                {
                    var fileSystems = FileSystemManager.DetectFileSystems(volume);

                    var fileSystem = Assert.Single(fileSystems);
                    Assert.Equal("FAT", fileSystem.Name);

                    using (FatFileSystem fat = (FatFileSystem)fileSystem.Open(volume))
                    {
                        Assert.True(fat.FileExists("hello.txt"));

                        using (Stream helloStream = fat.OpenFile("hello.txt", FileMode.Open, FileAccess.Read))
                        using (MemoryStream copyStream = new MemoryStream())
                        {
                            Assert.NotEqual(0, helloStream.Length);
                            helloStream.CopyTo(copyStream);
                            Assert.Equal(helloStream.Length, copyStream.Length);

                            Assert.Equal("Hello, World!\n", Encoding.UTF8.GetString(copyStream.ToArray()));
                        }
                    }
                }
            }
        }
    }
}