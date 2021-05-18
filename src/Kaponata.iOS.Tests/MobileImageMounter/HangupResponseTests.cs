// <copyright file="HangupResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.MobileImageMounter;
using Xunit;

namespace Kaponata.iOS.Tests.MobileImageMounter
{
    /// <summary>
    /// Tests the <see cref="HangupResponse"/> class.
    /// </summary>
    public class HangupResponseTests
    {
        /// <summary>
        /// <see cref="HangupResponse.FromDictionary(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void FromDictionary_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Goodbye", true);

            var response = new HangupResponse();
            response.FromDictionary(dict);

            Assert.Null(response.DetailedError);
            Assert.Null(response.Error);
            Assert.Null(response.Status);
            Assert.True(response.Goodbye);
        }
    }
}
