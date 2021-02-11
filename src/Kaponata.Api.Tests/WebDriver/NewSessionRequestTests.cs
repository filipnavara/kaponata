// <copyright file="NewSessionRequestTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Newtonsoft.Json;
using Xunit;

namespace Kaponata.Api.Tests.WebDriver
{
    /// <summary>
    /// Tests the <see cref="NewSessionRequest"/> class.
    /// </summary>
    public class NewSessionRequestTests
    {
        /// <summary>
        /// <see cref="NewSessionRequest"/> values can be deserialized properly.
        /// </summary>
        [Fact]
        public void NewSessionRequest_Deserializes()
        {
            var json = @"{ 'capabilities': { 'alwaysMatch': { 'kaponata:platformName': 'android'}, firstMatch: [ { 'kaponata:browserName': 'firefox'} ] } }";
            var request = JsonConvert.DeserializeObject<NewSessionRequest>(json);

            Assert.NotNull(request.Capabilities);

            Assert.NotNull(request.Capabilities.AlwaysMatch);
            Assert.Collection(
                request.Capabilities.AlwaysMatch,
                c =>
                {
                    Assert.Equal("kaponata:platformName", c.Key);
                    Assert.Equal("android", c.Value);
                });

            Assert.NotNull(request.Capabilities.FirstMatch);
            Assert.Collection(
                request.Capabilities.FirstMatch,
                m =>
                {
                    Assert.Collection(
                        m,
                        c =>
                        {
                            Assert.Equal("kaponata:browserName", c.Key);
                            Assert.Equal("firefox", c.Value);
                        });
                });
        }
    }
}
