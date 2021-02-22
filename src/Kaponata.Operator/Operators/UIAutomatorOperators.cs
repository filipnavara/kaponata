// <copyright file="UIAutomatorOperators.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Android.Adb;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Operator.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Contains the operators for creating UIAutomator2-based sessions.
    /// </summary>
    public class UIAutomatorOperators
    {
        private const string ImageName = "quay.io/kaponata/appium-android:1.20.2";
        private const int AppiumPort = 4723;
        private const int AdbPort = 5037;

        /// <summary>
        /// Builds an operator which provides a UIAutomator2 driver pod for each <see cref="WebDriverSession"/> which uses
        /// the UIAutomator2 driver.
        /// </summary>
        /// <param name="services">
        /// A service collection from which to host services.
        /// </param>
        /// <returns>
        /// A configured operator.
        /// </returns>
        public static ChildOperatorBuilder<WebDriverSession, V1Pod> BuildPodOperator(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var kubernetes = services.GetRequiredService<KubernetesClient>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("UIAutomator2Operator");

            return new ChildOperatorBuilder(services)
                .CreateOperator("WebDriverSession-UIAutomator2Driver-PodOperator")
                .Watches<WebDriverSession>()
                .WithLabels(s => s.Metadata.Labels[Annotations.AutomationName] == Annotations.AutomationNames.UIAutomator2)
                .Creates<V1Pod>(
                    (session, pod) =>
                    {
                        pod.EnsureMetadata().EnsureLabels();
                        pod.Metadata.Labels.Add(Annotations.SessionName, session.Metadata.Name);

                        pod.Spec = new V1PodSpec()
                        {
                            Containers = new V1Container[]
                            {
                                new V1Container()
                                {
                                    Image = ImageName,
                                    Name = "appium",
                                    Args = new string[] { "/app/appium/build/lib/main.js" },
                                    Ports = new V1ContainerPort[]
                                    {
                                        new V1ContainerPort()
                                        {
                                             ContainerPort = AppiumPort,
                                             Name = "http",
                                        },
                                    },
                                    ReadinessProbe = new V1Probe()
                                    {
                                        HttpGet = new V1HTTPGetAction()
                                        {
                                            Path = "/wd/hub/status",
                                            Port = $"{AppiumPort}",
                                        },
                                    },
                                },
                                new V1Container()
                                {
                                    Image = ImageName,
                                    Name = "adb",
                                    Command = new string[] { "/bin/tini", "--", "/android/platform-tools/adb" },
                                    Args = new string[] { "-a", "-P", $"{AdbPort}", "server", "nodaemon" },
                                    Ports = new V1ContainerPort[]
                                    {
                                        new V1ContainerPort()
                                        {
                                             ContainerPort = AdbPort,
                                             Name = "adb",
                                        },
                                    },
                                    ReadinessProbe = new V1Probe()
                                    {
                                        TcpSocket = new V1TCPSocketAction()
                                        {
                                             Port = $"{AdbPort}",
                                        },
                                    },
                                },
                            },
                        };
                    })
                .CreatesSession(
                    AppiumPort,
                    async (context, cancellationToken) =>
                    {
                        var session = context.Parent;
                        var pod = context.Child;

                        var adbContext = context.Services.GetRequiredService<KubernetesAdbContext>();
                        adbContext.Pod = pod;

                        // First, connect the instance of adb running on the Appium pod to the Android device
                        var adbClient = context.Services.GetRequiredService<AdbClient>();

                        await adbClient.ConnectDeviceAsync(new DnsEndPoint(session.Spec.DeviceHost, 5555), cancellationToken).ConfigureAwait(false);
                        var devices = await adbClient.GetDevicesAsync(cancellationToken).ConfigureAwait(false);
                    });
        }
    }
}
