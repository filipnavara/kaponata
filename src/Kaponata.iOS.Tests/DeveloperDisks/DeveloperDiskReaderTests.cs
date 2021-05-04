// <copyright file="DeveloperDiskReaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperDisks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace Kaponata.iOS.Tests.DeveloperDisks
{
    /// <summary>
    /// Tests the <see cref="DeveloperDiskReader"/> class.
    /// </summary>
    public class DeveloperDiskReaderTests
    {
        /// <summary>
        /// Gets test data for the <see cref="GetVersionInformation_Works(string)"/> test.
        /// </summary>
        /// <returns>
        /// Test data for the <see cref="GetVersionInformation_Works(string)"/> test.
        /// </returns>
        public static IEnumerable<object[]> GetDeveloperDiskImages()
        {
            yield return new object[] { "TestAssets/udro-hfsplus.dmg" };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                foreach (var dir in Directory.GetDirectories("/Applications/Xcode.app/Content/Developer/Platforms/iPhoneOS.Platform/DeviceSupport/"))
                {
                    yield return new object[] { Path.Combine(dir, "DeveloperDiskImage.dmg") };
                }
            }
        }

        /// <summary>
        /// <see cref="DeveloperDiskReader.GetVersionInformation(Stream)"/> returns valid version information.
        /// </summary>
        /// <param name="path">
        /// The path to a developer disk image.
        /// </param>
        [Theory]
        [MemberData(nameof(GetDeveloperDiskImages))]
        public void GetVersionInformation_Works(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                var version = DeveloperDiskReader.GetVersionInformation(stream);

                Assert.Equal("iPhone OS", version.ProductName);
            }
        }
    }
}
