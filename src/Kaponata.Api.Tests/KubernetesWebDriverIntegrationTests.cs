// <copyright file="KubernetesWebDriverIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.WebDriver;
using Kaponata.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Integration tests for the <see cref="KubernetesWebDriver"/> class. Assumes the Fake operators have been deployed
    /// to the default namespace.
    /// </summary>
    public class KubernetesWebDriverIntegrationTests
    {
        private IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesWebDriverIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// A test output helper to use when logging.
        /// </param>
        public KubernetesWebDriverIntegrationTests(ITestOutputHelper output)
        {
            this.host =
                new HostBuilder()
                .ConfigureServices(
                    s =>
                    {
                        s.AddKubernetes();
                        s.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output);
                        });
                        s.AddSingleton<KubernetesWebDriver>();
                    })
                .Build();
        }

        /// <summary>
        /// A Fake session transitions to the ready phase, and you can request the page source using the standard
        /// Selenium client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Flaky in CI")]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task CreateFakeSession_IntegrationTest_Async()
        {
            var webDriver = this.host.Services.GetRequiredService<KubernetesWebDriver>();
            var logger = this.host.Services.GetRequiredService<ILogger<KubernetesWebDriverIntegrationTests>>();

            var response = await webDriver.CreateSessionAsync(
                new NewSessionRequest()
                {
                    Capabilities = new CapabilitiesRequest()
                    {
                        AlwaysMatch = new Dictionary<string, object>()
                        {
                            { "platformName", "fake" },
                            { "deviceName", "fake" },
                            { "app", "/app/appium-fake-driver/test/fixtures/app.xml" },
                            { "address", "localhost" },
                            { "port", 8181 },
                        },
                    },
                },
                default).ConfigureAwait(false);

            var newSession = Assert.IsType<NewSessionResponse>(response.Value);
            logger.LogInformation($"Connecting to session {newSession.SessionId}");

            var client = new AttachableRemoteWebDriver(new Uri("http://localhost/wd/hub/"), newSession.SessionId);
            var source = client.PageSource;
            Assert.NotNull(source);

            await webDriver.DeleteSessionAsync(newSession.SessionId, default).ConfigureAwait(false);
        }
    }
}
