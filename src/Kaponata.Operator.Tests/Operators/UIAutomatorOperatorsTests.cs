// <copyright file="UIAutomatorOperatorsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Android.Adb;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Operators;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="UIAutomatorOperators"/> class.
    /// </summary>
    public class UIAutomatorOperatorsTests
    {
        private readonly Mock<KubernetesClient> kubernetes;
        private readonly Mock<NamespacedKubernetesClient<V1Pod>> podClient;
        private readonly Mock<NamespacedKubernetesClient<MobileDevice>> deviceClient;
        private readonly IHost host;
        private readonly Mock<AdbClient> adbClient = new Mock<AdbClient>(MockBehavior.Strict);

        /// <summary>
        /// Initializes a new instance of the <see cref="UIAutomatorOperatorsTests"/> class.
        /// </summary>
        /// <param name="output">
        /// An output helper to use when logging.
        /// </param>
        public UIAutomatorOperatorsTests(ITestOutputHelper output)
        {
            this.kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            this.podClient = this.kubernetes.WithClient<V1Pod>();
            this.deviceClient = this.kubernetes.WithClient<MobileDevice>();

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

                    services.AddScoped((p) => this.adbClient.Object);
                    services.AddScoped<KubernetesAdbContext>();
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// <see cref="UIAutomatorOperators.BuildPodOperator(IServiceProvider)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void BuildPodOperator_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("services", () => UIAutomatorOperators.BuildPodOperator(null));
        }

        /// <summary>
        /// The parent label selector and the child labels are configured correctly.
        /// </summary>
        [Fact]
        public void Operator_HasCorrectLabels()
        {
            var builder = UIAutomatorOperators.BuildPodOperator(this.host.Services);

            Assert.Equal("kaponata.io/automation-name=uiautomator2", builder.Configuration.ParentLabelSelector);
            Assert.Collection(
                builder.Configuration.ChildLabels,
                c =>
                {
                    Assert.Equal("app.kubernetes.io/managed-by", c.Key);
                    Assert.Equal("WebDriverSession-UIAutomator2Driver-PodOperator", c.Value);
                });
        }

        /// <summary>
        /// The session pod is configured correctly.
        /// </summary>
        [Fact]
        public void Operator_ConfiguresPod()
        {
            var builder = UIAutomatorOperators.BuildPodOperator(this.host.Services);

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                },
            };

            var child = new V1Pod();

            builder.ChildFactory(session, child);

            Assert.Collection(
                child.Metadata.Labels,
                a =>
                {
                    Assert.Equal("kaponata.io/session-name", a.Key);
                    Assert.Equal("my-session", a.Value);
                });

            Assert.Collection(
                child.Spec.Containers,
                c =>
                {
                    Assert.Equal("quay.io/kaponata/appium-android:1.20.2", c.Image);
                    Assert.Equal("appium", c.Name);
                    Assert.Collection(
                        c.Args,
                        a => Assert.Equal("/app/appium/build/lib/main.js", a));

                    var port = Assert.Single(c.Ports);
                    Assert.Equal(4723, port.ContainerPort);
                    Assert.Equal("http", port.Name);

                    Assert.Equal("/wd/hub/status", c.ReadinessProbe.HttpGet.Path);
                    Assert.Equal("4723", c.ReadinessProbe.HttpGet.Port);
                },
                c =>
                {
                    Assert.Equal("quay.io/kaponata/appium-android:1.20.2", c.Image);
                    Assert.Equal("adb", c.Name);
                    Assert.Collection(
                        c.Command,
                        a => Assert.Equal("/bin/tini", a),
                        a => Assert.Equal("--", a),
                        a => Assert.Equal("/android/platform-tools/adb", a));
                    Assert.Collection(
                        c.Args,
                        a => Assert.Equal("-a", a),
                        a => Assert.Equal("-P", a),
                        a => Assert.Equal("5037", a),
                        a => Assert.Equal("server", a),
                        a => Assert.Equal("nodaemon", a));

                    var port = Assert.Single(c.Ports);
                    Assert.Equal(5037, port.ContainerPort);
                    Assert.Equal("adb", port.Name);

                    Assert.Equal("5037", c.ReadinessProbe.TcpSocket.Port);
                });
        }

        /// <summary>
        /// <see cref="UIAutomatorOperators"/> connects the adb instance in the pod running Appium to the emulator
        /// on which the test is being executed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ConnectsDevice_Async()
        {
            var builder = UIAutomatorOperators.BuildPodOperator(this.host.Services);
            var feedbackLoop = Assert.Single(builder.FeedbackLoops);

            var session = new WebDriverSession()
            {
                Spec = new WebDriverSessionSpec()
                {
                    Capabilities = "{}",
                    DeviceHost = "1.2.3.4",
                },
            };

            var pod = new V1Pod()
            {
                Status = new V1PodStatus()
                {
                    Phase = "Running",
                    ContainerStatuses = new V1ContainerStatus[] { },
                },
            };

            var adbStream = new Mock<Stream>();
            this.kubernetes
                .Setup(c => c.ConnectToPodPortAsync(pod, 5037, It.IsAny<CancellationToken>()))
                .ReturnsAsync(adbStream.Object);

            var context = new ChildOperatorContext<WebDriverSession, V1Pod>(
                parent: session,
                child: pod,
                this.host.Services);

            this.adbClient
                .Setup(c => c.ConnectDeviceAsync(new DnsEndPoint("1.2.3.4", 5555), default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            this.adbClient
                .Setup(c => c.GetDevicesAsync(default))
                .Returns(Task.FromResult<IList<DeviceData>>(new List<DeviceData>()))
                .Verifiable();

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
                k => k.CreatePodHttpClient(context.Child, 4723))
                .Returns(client);

            var patch = await feedbackLoop(context, default).ConfigureAwait(false);
            Assert.NotNull(patch);

            this.adbClient.Verify();

            Assert.Collection(
                patch.ParentFeedback.Operations,
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
                    Assert.Equal(4723, o.value);
                },
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/capabilities", o.path);
                    Assert.Equal("{}", o.value);
                });
            Assert.Null(patch.ChildFeedback);
        }
    }
}
