// <copyright file="LookupImageResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.MobileImageMounter;
using System.IO;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="LookupImageResponse"/> class.
    /// </summary>
    public class LookupImageResponseTests
    {
        /// <summary>
        /// <see cref="LookupImageResponse.FromDictionary(NSDictionary)"/> works correctly.
        /// </summary>
        /// <param name="path">
        /// The path to the file which contains the serialized response.
        /// </param>
        /// <param name="hasImage">
        /// A value indicating whether the response indicates the image is mounted.
        /// </param>
        [Theory]
        [InlineData("MobileImageMounter/noImagesResponse.bin", false)]
        [InlineData("MobileImageMounter/imageLookupResponse.bin", true)]
        public void FromDictionary(string path, bool hasImage)
        {
            var response = new LookupImageResponse();

            NSDictionary dictionary = (NSDictionary)XmlPropertyListParser.Parse(File.ReadAllBytes(path));
            response.FromDictionary(dictionary);
            Assert.Null(response.DetailedError);
            Assert.Null(response.Error);
            Assert.Equal(hasImage, response.ImageSignature != null);
            Assert.Equal("Complete", response.Status);
        }
    }
}
