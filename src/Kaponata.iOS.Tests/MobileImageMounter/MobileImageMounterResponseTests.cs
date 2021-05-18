// <copyright file="MobileImageMounterResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.MobileImageMounter;
using System.IO;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="MobileImageMounterResponse"/> class.
    /// </summary>
    public class MobileImageMounterResponseTests
    {
        /// <summary>
        /// <see cref="MobileImageMounterResponse.FromDictionary(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadError_Works()
        {
            var dict = (NSDictionary)XmlPropertyListParser.Parse(File.ReadAllBytes("MobileImageMounter/mountImageAlreadyMountedResponse.bin"));
            MobileImageMounterResponse response = new MobileImageMounterResponse();
            response.FromDictionary(dict);

            Assert.NotNull(response.DetailedError);
            Assert.Equal("ImageMountFailed", response.Error);
            Assert.Null(response.Status);
        }
    }
}
