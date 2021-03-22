// <copyright file="EntitlementsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.DeveloperProfiles;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="Entitlements"/> class.
    /// </summary>
    public class EntitlementsTests
    {
        /// <summary>
        /// <see cref="Entitlements.Read(NSDictionary)"/> throws when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void Read_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Entitlements.Read(null));
        }
    }
}
