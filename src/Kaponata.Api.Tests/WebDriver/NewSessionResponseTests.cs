// <copyright file="NewSessionResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="NewSessionResponse"/> class.
    /// </summary>
    public class NewSessionResponseTests
    {
        /// <summary>
        /// <see cref="NewSessionResponse"/> data can be serialized correctly.
        /// </summary>
        [Fact]
        public void NewSessionResponse_SerializesCorrectly()
        {
            var response = new NewSessionResponse()
            {
                Capabilities = new Dictionary<string, object>()
                {
                     { "acceptInsecureCerts", false },
                     { "browserName", "firefox" },
                },
                SessionId = "1234567890",
            };

            var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() } });
            Assert.Equal("{\"sessionId\":\"1234567890\",\"capabilities\":{\"acceptInsecureCerts\":false,\"browserName\":\"firefox\"}}", json);
        }
    }
}
