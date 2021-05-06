// <copyright file="SystemVersionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.DeveloperDisks;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.DeveloperDisks
{
    /// <summary>
    /// Tests the <see cref="SystemVersion"/> class.
    /// </summary>
    public class SystemVersionTests
    {
        /// <summary>
        /// The <see cref="SystemVersion.FromDictionary(NSDictionary)"/> method throws when passed
        /// <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void FromDictionary_ThrowsOnNull()
        {
            var version = new SystemVersion();
            Assert.Throws<ArgumentNullException>(() => version.FromDictionary(null));
        }

        /// <summary>
        /// <see cref="SystemVersion.FromDictionary(NSDictionary)"/> correctly parses data.
        /// </summary>
        [Fact]
        public void FromDictionary_Works()
        {
            // Create a dict with sample data seen in e.g.
            // https://github.com/WebKit/WebKit/blob/master/Tools/CISupport/ews-build/steps_unittest.py#L3783-L3789
            var dict = new NSDictionary();
            dict.Add("BuildID", "BB4C82AE-5F8A-11EA-A1A5-838AD03DDE06");
            dict.Add("ProductBuildVersion", "17E255");
            dict.Add("ProductCopyright", "1983-2020 Apple Inc.");
            dict.Add("ProductName", "iPhone OS");
            dict.Add("ProductVersion", "13.4");

            var version = new SystemVersion();
            version.FromDictionary(dict);

            Assert.Equal(new Guid("BB4C82AE-5F8A-11EA-A1A5-838AD03DDE06"), version.BuildID);
            Assert.Equal(AppleVersion.Parse("17E255"), version.ProductBuildVersion);
            Assert.Equal("1983-2020 Apple Inc.", version.ProductCopyright);
            Assert.Equal("iPhone OS", version.ProductName);
            Assert.Equal(new Version("13.4"), version.ProductVersion);
        }

        /// <summary>
        /// <see cref="SystemVersion.FromDictionary(NSDictionary)"/> correctly parses data.
        /// </summary>
        [Fact]
        public void FromDictionary_NoBuildId_Works()
        {
            // Create a dict with sample data seen in e.g.
            // https://github.com/WebKit/WebKit/blob/master/Tools/CISupport/ews-build/steps_unittest.py#L3783-L3789
            var dict = new NSDictionary();
            dict.Add("ProductBuildVersion", "17E255");
            dict.Add("ProductCopyright", "1983-2020 Apple Inc.");
            dict.Add("ProductName", "iPhone OS");
            dict.Add("ProductVersion", "13.4");

            var version = new SystemVersion();
            version.FromDictionary(dict);

            Assert.Equal(Guid.Empty, version.BuildID);
            Assert.Equal(AppleVersion.Parse("17E255"), version.ProductBuildVersion);
            Assert.Equal("1983-2020 Apple Inc.", version.ProductCopyright);
            Assert.Equal("iPhone OS", version.ProductName);
            Assert.Equal(new Version("13.4"), version.ProductVersion);
        }

        /// <summary>
        /// <see cref="SystemVersion.ToDictionary"/> works when <see cref="SystemVersion.BuildID"/>
        /// is not set.
        /// </summary>
        [Fact]
        public void ToDictionary_NoBuildIDs_Works()
        {
            var version = new SystemVersion()
            {
                ProductBuildVersion = AppleVersion.Parse("17E255"),
                ProductCopyright = "1983-2020 Apple Inc.",
                ProductName = "iPhone OS",
                ProductVersion = new Version(13, 4),
            };

            var dict = version.ToDictionary();
            Assert.Collection(
                dict,
                e =>
                {
                    Assert.Equal("ProductBuildVersion", e.Key);
                    Assert.Equal("17E255", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductCopyright", e.Key);
                    Assert.Equal("1983-2020 Apple Inc.", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductName", e.Key);
                    Assert.Equal("iPhone OS", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductVersion", e.Key);
                    Assert.Equal("13.4", e.Value.ToObject());
                });
        }

        /// <summary>
        /// <see cref="SystemVersion.ToDictionary"/> works.
        /// </summary>
        [Fact]
        public void ToDictionary_Works()
        {
            var version = new SystemVersion()
            {
                BuildID = new Guid("BB4C82AE-5F8A-11EA-A1A5-838AD03DDE06"),
                ProductBuildVersion = AppleVersion.Parse("17E255"),
                ProductCopyright = "1983-2020 Apple Inc.",
                ProductName = "iPhone OS",
                ProductVersion = new Version(13, 4),
            };

            var dict = version.ToDictionary();
            Assert.Collection(
                dict,
                e =>
                {
                    Assert.Equal("BuildID", e.Key);
                    Assert.Equal("BB4C82AE-5F8A-11EA-A1A5-838AD03DDE06", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductBuildVersion", e.Key);
                    Assert.Equal("17E255", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductCopyright", e.Key);
                    Assert.Equal("1983-2020 Apple Inc.", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductName", e.Key);
                    Assert.Equal("iPhone OS", e.Value.ToObject());
                },
                e =>
                {
                    Assert.Equal("ProductVersion", e.Key);
                    Assert.Equal("13.4", e.Value.ToObject());
                });
        }
    }
}
