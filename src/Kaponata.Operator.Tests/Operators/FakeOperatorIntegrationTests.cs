// <copyright file="FakeOperatorIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Kaponata.Operator.Operators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Integration tests for the <see cref="FakeOperators"/> class.
    /// </summary>
    public class FakeOperatorIntegrationTests
    {
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOperatorIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// A <see cref="ITestOutputHelper"/> which captures test output.
        /// </param>
        public FakeOperatorIntegrationTests(ITestOutputHelper output)
        {
            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddKubernetes();
                    services.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output);
                        });
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// Executes an integration test for the <see cref="FakeOperators"/> class.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task FakeOperatorLifecycle_IntegrationTest_Success_Async()
        {
            const string name = "fake-operator-success-lifecycle";

            var kubernetes = this.host.Services.GetRequiredService<KubernetesClient>();
            var loggerFactory = this.host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(name);

            var podOperator = FakeOperators.BuildPodOperator(this.host).Build();
            var serviceOperator = FakeOperators.BuildServiceOperator(this.host).Build();
            var ingressOperator = FakeOperators.BuildIngressOperator(this.host).Build();

            // Create a session and monitor the progress of the session.
            var sessionClient = kubernetes.GetClient<WebDriverSession>();
            var podClient = kubernetes.GetClient<V1Pod>();
            var serviceClient = kubernetes.GetClient<V1Service>();
            var ingressClient = kubernetes.GetClient<V1Ingress>();

            await Task.WhenAll(
                sessionClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                podClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                serviceClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                ingressClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);

            var sessionWatcher = sessionClient.WatchAsync(
                "default",
                $"metadata.name={name}",
                null,
                null,
                (eventType, session) =>
                {
                    if (session.Metadata.Name != name)
                    {
                        return Task.FromResult(WatchResult.Continue);
                    }

                    switch (eventType)
                    {
                        case k8s.WatchEventType.Modified when session.Status != null:
                            logger.LogInformation($"{DateTime.Now}: Session: {session.Status.SessionReady}, Service: {session.Status.ServiceReady} Ingress: {session.Status.IngressReady}.");
                            if (session.Status.IngressReady && session.Status.ServiceReady && session.Status.SessionReady)
                            {
                                return Task.FromResult(WatchResult.Stop);
                            }

                            break;
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                default);

            await Task.WhenAll(
                podOperator.StartAsync(default),
                serviceOperator.StartAsync(default),
                ingressOperator.StartAsync(default)).ConfigureAwait(false);

            // Create the session
            await sessionClient.CreateAsync(
                new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = new Dictionary<string, string>()
                        {
                            { Annotations.AutomationName, Annotations.AutomationNames.Fake },
                        },
                        NamespaceProperty = "default",
                        Name = name,
                    },
                    Spec = new WebDriverSessionSpec()
                    {
                        Capabilities = JsonConvert.SerializeObject(
                            new
                            {
                                alwaysMatch = new
                                {
                                    platformName = "Fake",
                                    deviceName = "Fake",
                                    app = "/app/appium-fake-driver/test/fixtures/app.xml",
                                    address = "localhost",
                                    port = 8181,
                                },
                            }),
                    },
                },
                default).ConfigureAwait(false);

            await Task.WhenAny(
                sessionWatcher,
                Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);

            Assert.True(sessionWatcher.IsCompletedSuccessfully, "Failed to properly configure the session within a timeout of 1 minute");

            var session = await sessionClient.TryReadAsync("default", name, default).ConfigureAwait(false);

            Assert.NotNull(session.Status.Capabilities);
            Assert.Null(session.Status.Data);
            Assert.Null(session.Status.Error);
            Assert.Null(session.Status.Message);
            Assert.Null(session.Status.StackTrace);
            Assert.NotNull(session.Status.SessionId);

            await Task.WhenAll(
                podOperator.StopAsync(default),
                serviceOperator.StopAsync(default),
                ingressOperator.StopAsync(default)).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an integration test for the <see cref="FakeOperators"/> class, in a scenario where the session cannot be
        /// provisioned because the capabilities are invalid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task FakeOperatorLifecycle_IntegrationTest_Failure_Async()
        {
            const string name = "fake-operator-failure-lifecycle";

            var kubernetes = this.host.Services.GetRequiredService<KubernetesClient>();
            var loggerFactory = this.host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(name);

            var podOperator = FakeOperators.BuildPodOperator(this.host).Build();
            var serviceOperator = FakeOperators.BuildServiceOperator(this.host).Build();
            var ingressOperator = FakeOperators.BuildIngressOperator(this.host).Build();

            // Create a session and monitor the progress of the session.
            var sessionClient = kubernetes.GetClient<WebDriverSession>();
            var podClient = kubernetes.GetClient<V1Pod>();
            var serviceClient = kubernetes.GetClient<V1Service>();
            var ingressClient = kubernetes.GetClient<V1Ingress>();

            await Task.WhenAll(
                sessionClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                podClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                serviceClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default),
                ingressClient.TryDeleteAsync("default", name, TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);

            var sessionWatcher = sessionClient.WatchAsync(
                "default",
                $"metadata.name={name}",
                null,
                null,
                (eventType, session) =>
                {
                    if (session.Metadata.Name != name)
                    {
                        return Task.FromResult(WatchResult.Continue);
                    }

                    switch (eventType)
                    {
                        case k8s.WatchEventType.Modified when session.Status != null:
                            logger.LogInformation($"{DateTime.Now}: Session: {session.Status.SessionReady}, Service: {session.Status.ServiceReady} Ingress: {session.Status.IngressReady}.");
                            if (session.Status.Error != null)
                            {
                                return Task.FromResult(WatchResult.Stop);
                            }

                            break;
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                default);

            await Task.WhenAll(
                podOperator.StartAsync(default),
                serviceOperator.StartAsync(default),
                ingressOperator.StartAsync(default)).ConfigureAwait(false);

            // Create the session
            await sessionClient.CreateAsync(
                new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = new Dictionary<string, string>()
                        {
                            { Annotations.AutomationName, Annotations.AutomationNames.Fake },
                        },
                        NamespaceProperty = "default",
                        Name = name,
                    },
                    Spec = new WebDriverSessionSpec()
                    {
                        Capabilities = JsonConvert.SerializeObject(
                            new
                            {
                                alwaysMatch = new
                                {
                                    platformName = "Fake",
                                    deviceName = "Fake",

                                    // This is a required capability; leaving it out will cause the session to fail
                                    app = (string)null,
                                    address = "localhost",
                                    port = 8181,
                                },
                            }),
                    },
                },
                default).ConfigureAwait(false);

            await Task.WhenAny(
                sessionWatcher,
                Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);

            Assert.True(sessionWatcher.IsCompletedSuccessfully, "Failed to properly configure the session within a timeout of 1 minute");

            var session = await sessionClient.TryReadAsync("default", name, default).ConfigureAwait(false);

            Assert.Null(session.Status.Capabilities);
            Assert.Null(session.Status.Data);
            Assert.Equal("session not created", session.Status.Error);
            Assert.Equal("A new session could not be created. Details: The desiredCapabilities object was not valid for the following reason(s): 'app' can't be blank", session.Status.Message);
            Assert.StartsWith("SessionNotCreatedError: A new session could not be created.", session.Status.StackTrace);
            Assert.Null(session.Status.SessionId);

            await Task.WhenAll(
                podOperator.StopAsync(default),
                serviceOperator.StopAsync(default),
                ingressOperator.StopAsync(default)).ConfigureAwait(false);
        }
    }
}
