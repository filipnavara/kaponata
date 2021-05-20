// <copyright file="StartServiceResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="StartServiceResponse"/> class.
    /// </summary>
    public class StartServiceResponseTests
    {
        /// <summary>
        /// <see cref="StartServiceResponse.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void FromDictionary_ValidatesArguments()
        {
            var response = new StartServiceResponse();
            Assert.Throws<ArgumentNullException>(() => response.FromDictionary(null));
        }

        /// <summary>
        /// <see cref="StartServiceResponse.FromDictionary(NSDictionary)"/> works correctly.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("EnableServiceSSL", true);
            dict.Add("Port", 1234);
            dict.Add("Request", "StartService");
            dict.Add("Service", "my-service");

            var response = new StartServiceResponse();
            response.FromDictionary(dict);

            Assert.True(response.EnableServiceSSL);
            Assert.Null(response.Error);
            Assert.Equal(1234, response.Port);
            Assert.Equal("StartService", response.Request);
            Assert.Equal("my-service", response.Service);
        }
    }
}
