// <copyright file="StartupTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests
{
    /// <summary>
    /// Tests the <see cref="Startup"/> class.
    /// </summary>
    public class StartupTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupTests"/> class.
        /// </summary>
        /// <param name="factory">
        /// A <see cref="WebApplicationFactory{TEntryPoint}"/> which is used to generate
        /// the web application.
        /// </param>
        public StartupTests(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// <see cref="Startup.ConfigureServices(IServiceCollection)"/> configures the health checks.
        /// </summary>
        [Fact]
        public void ConfigureServices_AddsHealthCheck()
        {
            IServiceCollection services = new ServiceCollection();

            var startup = new Startup();
            startup.ConfigureServices(services);

            Assert.Contains(services, s => s.ServiceType == typeof(HealthCheckService));
        }

        /// <summary>
        /// The root, health and alive probes return a success status code, and embed the version
        /// number in a header.
        /// </summary>
        /// <param name="url">
        /// The URL being tested.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("/")]
        [InlineData("/health/ready")]
        [InlineData("/health/alive")]
        public async Task StatusEndPoints_ReturnSuccessAndVersion_Async(string url)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/plain", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Headers.TryGetValues("X-Kaponata-Version", out var values));
            Assert.Equal(ThisAssembly.AssemblyInformationalVersion, Assert.Single(values));
        }
    }
}
