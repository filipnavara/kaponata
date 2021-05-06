// <copyright file="ErrorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System.Text.Json;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="Error" /> class.
    /// </summary>
    public class ErrorTests
    {
        /// <summary>
        /// A <see cref="Error"/> object can be deserialized properly from JSON.
        /// </summary>
        [Fact]
        public void Deserialize_Works()
        {
            const string json = "{\"errors\":[{\"code\":\"DIGEST_INVALID\",\"message\":\"provided digest did not match uploaded content\",\"detail\":\"digest parsing failed\"}]}";
            var errors = JsonSerializer.Deserialize<ErrorList>(json);

            Assert.NotNull(errors);
            var error = Assert.Single(errors.Errors);
            Assert.Equal("DIGEST_INVALID", error.Code);
            Assert.Equal("provided digest did not match uploaded content", error.Message);
            Assert.Equal("digest parsing failed", error.Detail.ToString());
        }
    }
}
