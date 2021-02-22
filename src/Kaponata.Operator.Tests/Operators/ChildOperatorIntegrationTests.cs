// <copyright file="ChildOperatorIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Kaponata.Operator.Operators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Integration tests for the <see cref="ChildOperator{TParent, TChild}"/> class.
    /// </summary>
    [Collection("OperatorTests")]
    public class ChildOperatorIntegrationTests
    {
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorIntegrationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// A <see cref="ITestOutputHelper"/> which captures test output.
        /// </param>
        public ChildOperatorIntegrationTests(ITestOutputHelper output)
        {
            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddKubernetes(@namespace: "tests");
                    services.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output, LogConfig.Default);
                        });
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}"/> creates a new child object for items which it monitors, but does not act
        /// on items which do not match the label or field selectors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("TestCategory", "IntegrationTest")]
        public async Task Session_CreatesAndDeletesPod_Async()
        {
            const string name = "session-createsanddeletespod-async";
            Debug.Assert(FormatName(nameof(this.Session_CreatesAndDeletesPod_Async)) == name, "Use naming conventions");

            var configuration = new ChildOperatorConfiguration(name)
            {
                ParentLabelSelector = LabelSelector.Create<WebDriverSession>(
                    s => s.Metadata.Labels[Annotations.ManagedBy] == name
                    && s.Metadata.Labels[Annotations.AutomationName] == Annotations.AutomationNames.Fake),
            };

            var podCreated = new TaskCompletionSource<V1Pod>();
            var podRunning = new TaskCompletionSource<V1Pod>();
            var podDeleted = new TaskCompletionSource<V1Pod>();

            var kubernetes = this.host.Services.GetRequiredService<KubernetesClient>();
            var podClient = kubernetes.GetClient<V1Pod>();

            var sessionClient = kubernetes.GetClient<WebDriverSession>();
            var logger = this.host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(name);

            // Delete all objects which may have been previously created by this test.
            await Task.WhenAll(
                sessionClient.TryDeleteAsync($"{name}-empty", TimeSpan.FromMinutes(1), default),
                sessionClient.TryDeleteAsync($"{name}-fake", TimeSpan.FromMinutes(1), default),
                podClient.TryDeleteAsync($"{name}-empty", TimeSpan.FromMinutes(1), default),
                podClient.TryDeleteAsync($"{name}-fake", TimeSpan.FromMinutes(1), default)).ConfigureAwait(false);

            var podWatcher =
                podClient.WatchAsync(
                    fieldSelector: null,
                    labelSelector: $"{Annotations.ManagedBy}={name}",
                    null,
                    (eventType, pod) =>
                    {
                        logger.LogInformation($"Got an added {eventType}  event for pod {pod}", eventType, pod.Metadata.Name);
                        switch (eventType)
                        {
                            case k8s.WatchEventType.Added:
                                podCreated.TrySetResult(pod);
                                break;

                            case k8s.WatchEventType.Modified when pod.Status.Phase == "Running" && pod.Status.ContainerStatuses.All(c => c.Ready):
                                podRunning.TrySetResult(pod);
                                break;

                            case k8s.WatchEventType.Deleted:
                                podDeleted.TrySetResult(pod);
                                return Task.FromResult(WatchResult.Stop);
                        }

                        return Task.FromResult(WatchResult.Continue);
                    },
                    default);

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes,
                configuration,
                (session) => true,
                (mobileDevice, pod) =>
                {
                    pod.Spec = new V1PodSpec()
                    {
                        Containers = new V1Container[]
                        {
                            new V1Container()
                            {
                                Name = "fake-driver",
                                Image = "quay.io/kaponata/fake-driver:2.0.1",
                            },
                        },
                    };
                },
                new Collection<FeedbackLoop<WebDriverSession, V1Pod>>(),
                this.host.Services.GetRequiredService<ILogger<ChildOperator<WebDriverSession, V1Pod>>>(),
                this.host.Services))
            {
                // Start the operator
                await @operator.StartAsync(default).ConfigureAwait(false);

                // Create two new sessions, one which uses the fake automation and another which uses dummy automation.
                // A pod should be created for the fake session, but not the dummy session
                var emptySession = await sessionClient.CreateAsync(
                    new WebDriverSession()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = $"{name}-empty",
                            Labels = new Dictionary<string, string>()
                            {
                                { Annotations.ManagedBy, name },
                                { Annotations.AutomationName, "dummy" },
                            },
                        },
                    },
                    default).ConfigureAwait(false);

                var fakeSession = await sessionClient.CreateAsync(
                    new WebDriverSession()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name = $"{name}-fake",
                            Labels = new Dictionary<string, string>()
                            {
                                { Annotations.ManagedBy, name },
                                { Annotations.AutomationName, Annotations.AutomationNames.Fake },
                            },
                        },
                    },
                    default).ConfigureAwait(false);

                await Task.WhenAny(podCreated.Task, Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);
                Assert.True(podCreated.Task.IsCompleted, "Failed to create the pod within a timespan of 1 minute");
                var createdPod = await podCreated.Task.ConfigureAwait(false);
                var ownerReference = Assert.Single(createdPod.Metadata.OwnerReferences);
                logger.LogInformation($"Created pod: {JsonConvert.SerializeObject(createdPod)}");
                logger.LogInformation($"Owner reference: {JsonConvert.SerializeObject(ownerReference)}");
                Assert.Equal($"{name}-fake", createdPod.Metadata.Name);

                await Task.WhenAny(podRunning.Task, Task.Delay(TimeSpan.FromMinutes(2))).ConfigureAwait(false);
                Assert.True(podRunning.Task.IsCompleted, "Failed to create start the pod within a timespan of 2 minutes");
                var runningPod = await podCreated.Task.ConfigureAwait(false);
                logger.LogInformation($"Pod is running: {JsonConvert.SerializeObject(runningPod)}");

                // Deleting the sessions should result in the associated pod being deleted, too.
                await sessionClient.DeleteAsync(emptySession, new V1DeleteOptions(propagationPolicy: "Foreground"), TimeSpan.FromMinutes(1), default).ConfigureAwait(false);
                await sessionClient.DeleteAsync(fakeSession, new V1DeleteOptions(propagationPolicy: "Foreground"), TimeSpan.FromMinutes(1), default).ConfigureAwait(false);
                await Task.WhenAny(podWatcher, podDeleted.Task, Task.Delay(TimeSpan.FromMinutes(1))).ConfigureAwait(false);

                if (!podDeleted.Task.IsCompleted)
                {
                    // Get some additional information - does the pod still exist (and is the timeout too low), or has the
                    // pod been deleted and were we not notified?
                    logger.LogWarning($"Failed to create the pod within a timespan of 1 minute");
                    var updatedPod = await podClient.TryReadAsync(createdPod.Metadata.Name, default).ConfigureAwait(false);

                    if (updatedPod == null)
                    {
                        logger.LogInformation("The pod has been deleted");
                    }
                    else
                    {
                        logger.LogInformation($"Refreshed pod details: {JsonConvert.SerializeObject(updatedPod)}");
                    }
                }

                Assert.True(podDeleted.Task.IsCompleted, "Failed to delete the pod within a timespan of 1 minute");
                var deletedPod = await podDeleted.Task.ConfigureAwait(false);
                Assert.Equal($"{name}-fake", deletedPod.Metadata.Name);

                // Stop the operator.
                await @operator.StopAsync(default).ConfigureAwait(false);
            }
        }

        private static string FormatNameCamelCase(string value)
        {
            return value.Replace("_", "-");
        }

        private static string FormatName(string value)
        {
            return value.ToLower().Replace("_", "-");
        }
    }
}
