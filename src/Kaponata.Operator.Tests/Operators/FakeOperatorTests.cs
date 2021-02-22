// <copyright file="FakeOperatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Operator.Operators;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="FakeOperators"/> class.
    /// </summary>
    public partial class FakeOperatorTests
    {
        private readonly Mock<KubernetesClient> kubernetes;
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOperatorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// A test output helper which can be used when logging.
        /// </param>
        public FakeOperatorTests(ITestOutputHelper output)
        {
            this.kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);

            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddSingleton(this.kubernetes.Object);
                    services.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output);
                        });
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// Data for the <see cref="BuildPodOperator_NoFeedback_Async(ChildOperatorContext{WebDriverSession, V1Pod})"/> theory.
        /// </summary>
        /// <returns>
        /// Test data.
        /// </returns>
        public static IEnumerable<object[]> BuildPodOperator_NoFeedback_Data()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(Mock.Of<KubernetesClient>())
                    .AddLogging()
                    .BuildServiceProvider();

            // The session has no requested capabilities.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession(),
                   null,
                   services),
            };

            // The session has no requested capabilities.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec(),
                   },
                   null,
                   services),
            };

            // The session already has a session id
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus()
                       {
                           SessionId = "abc",
                       },
                   },
                   null,
                   services),
            };

            // The pod does not exist.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus(),
                   },
                   null,
                   services),
            };

            // The pod is not ready.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus(),
                   },
                   new V1Pod(),
                   services),
            };

            // The pod is not ready.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus(),
                   },
                   new V1Pod()
                   {
                       Status = new V1PodStatus(),
                   },
                   services),
            };

            // The pod is not ready.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus(),
                   },
                   new V1Pod()
                   {
                       Status = new V1PodStatus()
                       {
                           Phase = "Pending",
                       },
                   },
                   services),
            };

            // The pod is not ready.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Pod>(
                   new WebDriverSession()
                   {
                       Spec = new WebDriverSessionSpec()
                       {
                           Capabilities = "test",
                       },
                       Status = new WebDriverSessionStatus(),
                   },
                   new V1Pod()
                   {
                       Status = new V1PodStatus()
                       {
                           Phase = "Running",
                           ContainerStatuses = new V1ContainerStatus[]
                           {
                               new V1ContainerStatus()
                               {
                                   Ready = false,
                               },
                           },
                       },
                   },
                   services),
            };
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> throws when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void BuildPodOperator_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>("services", () => FakeOperators.BuildPodOperator(null));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> correctly populates the standard properties.
        /// </summary>
        [Fact]
        public void BuildPodOperator_SimpleProperties_Test()
        {
            var builder = FakeOperators.BuildPodOperator(this.host.Services);

            // Name, namespace and labels
            Assert.Collection(
                builder.Configuration.ChildLabels,
                l =>
                {
                    Assert.Equal(Annotations.ManagedBy, l.Key);
                    Assert.Equal("WebDriverSession-FakeDriver-PodOperator", l.Value);
                });

            Assert.Equal("WebDriverSession-FakeDriver-PodOperator", builder.Configuration.OperatorName);
            Assert.Equal("kaponata.io/automation-name=fake", builder.Configuration.ParentLabelSelector);

            // Parent Filter: no filter, always returns true.
            Assert.True(builder.ParentFilter(null));

            // Child factory
            Assert.NotNull(builder.ChildFactory);

            // Feedback loop
            Assert.NotEmpty(builder.FeedbackLoops);
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> correctly configures the child pod.
        /// </summary>
        [Fact]
        public void BuildPodOperator_ConfiguresPod_Test()
        {
            var builder = FakeOperators.BuildPodOperator(this.host.Services);

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                },
            };

            var pod = new V1Pod();

            builder.ChildFactory(session, pod);

            Assert.Collection(
                pod.Metadata.Labels,
                l =>
                {
                    Assert.Equal(Annotations.SessionName, l.Key);
                    Assert.Equal("my-session", l.Value);
                });

            var container = Assert.Single(pod.Spec.Containers);

            Assert.Equal("quay.io/kaponata/fake-driver:2.0.1", container.Image);
            var port = Assert.Single(container.Ports);
            Assert.Equal(4774, port.ContainerPort);

            var probe = container.ReadinessProbe.HttpGet;
            Assert.Equal("/wd/hub/status", probe.Path);
            Assert.Equal(4774, probe.Port);
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> does nto provide any feedback when the sessions or pod are not ready.
        /// </summary>
        /// <param name="context">
        /// The context on which to operate.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(BuildPodOperator_NoFeedback_Data))]
        public async Task BuildPodOperator_NoFeedback_Async(ChildOperatorContext<WebDriverSession, V1Pod> context)
        {
            var builder = FakeOperators.BuildPodOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);
            Assert.Null(await feedback(context, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> provides correct feedback when session creation succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task BuildPodOperator_FeedbackSucceeds_Async()
        {
            var builder = FakeOperators.BuildPodOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);

            var context = new ChildOperatorContext<WebDriverSession, V1Pod>(
                new WebDriverSession()
                {
                    Spec = new WebDriverSessionSpec()
                    {
                        Capabilities = "{}",
                    },
                },
                new V1Pod()
                {
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[] { },
                    },
                },
                this.host.Services);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{value:{'sessionId':'1','capabilities':{}}}"),
            };

            var handler = new Mock<HttpClientHandler>();
            handler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var client = new HttpClient(handler.Object);
            client.BaseAddress = new Uri("http://webdriver/");

            this.kubernetes.Setup(
                k => k.CreatePodHttpClient(context.Child, 4774))
                .Returns(client);

            var result = await feedback(context, default).ConfigureAwait(false);
            Assert.Collection(
                result.Operations,
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status", o.path);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/sessionId", o.path);
                    Assert.Equal("1", o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/sessionReady", o.path);
                    Assert.Equal(true, o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/sessionPort", o.path);
                    Assert.Equal(4774, o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/capabilities", o.path);
                    Assert.Equal("{}", o.value);
                });

            handler.Verify();
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildPodOperator(IServiceProvider)"/> provides correct feedback when session creation fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task BuildPodOperator_FeedbackFails_Async()
        {
            var builder = FakeOperators.BuildPodOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);

            var context = new ChildOperatorContext<WebDriverSession, V1Pod>(
                new WebDriverSession()
                {
                    Spec = new WebDriverSessionSpec()
                    {
                        Capabilities = "{}",
                    },
                    Status = new WebDriverSessionStatus(),
                },
                new V1Pod()
                {
                    Status = new V1PodStatus()
                    {
                        Phase = "Running",
                        ContainerStatuses = new V1ContainerStatus[] { },
                    },
                },
                this.host.Services);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(@"{value:{'error':'session not created','message':'implementation defined','stacktrace':'stacktrace','data':'data'}}"),
            };

            var handler = new Mock<HttpClientHandler>();
            handler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var client = new HttpClient(handler.Object);
            client.BaseAddress = new Uri("http://webdriver/");

            this.kubernetes.Setup(
                k => k.CreatePodHttpClient(context.Child, 4774))
                .Returns(client);

            var result = await feedback(context, default).ConfigureAwait(false);
            Assert.Collection(
                result.Operations,
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/error", o.path);
                    Assert.Equal("session not created", o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/message", o.path);
                    Assert.Equal("implementation defined", o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/stacktrace", o.path);
                    Assert.Equal("stacktrace", o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/data", o.path);

                    // Data can be an arbitrary object, so a string is serialized with quotes.
                    Assert.Equal("\"data\"", o.value);
                });

            handler.Verify();
        }
    }
}
