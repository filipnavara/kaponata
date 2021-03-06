// <copyright file="FakeOperators.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Contains operators for provisioning <see cref="WebDriverSession"/> instances which use the Fake driver.
    /// </summary>
    public static class FakeOperators
    {
        private const string ImageName = "quay.io/kaponata/fake-driver:2.0.1";
        private const int Port = 4774;

        /// <summary>
        /// Builds an operator which provides a Fake driver pod for each <see cref="WebDriverSession"/> which uses
        /// the Fake driver.
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
            var logger = loggerFactory.CreateLogger("FakeOperator");

            return new ChildOperatorBuilder(services)
                .CreateOperator("WebDriverSession-FakeDriver-PodOperator")
                .Watches<WebDriverSession>()
                .WithLabels(s => s.Metadata.Labels[Annotations.AutomationName] == Annotations.AutomationNames.Fake)
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
                                    Name = "appium-fake-driver",
                                    Ports = new V1ContainerPort[]
                                    {
                                        new V1ContainerPort()
                                        {
                                             ContainerPort = Port,
                                             Name = "http",
                                        },
                                    },
                                    ReadinessProbe = new V1Probe()
                                    {
                                        HttpGet = new V1HTTPGetAction()
                                        {
                                            Path = "/wd/hub/status",
                                            Port = $"{Port}",
                                        },
                                    },
                                },
                            },
                        };
                    })
                .CreatesSession(appiumPort: Port);
        }

        /// <summary>
        /// Builds an operator which provisions ingress rules for <see cref="WebDriverSession"/> objects which
        /// use the Fake driver.
        /// </summary>
        /// <param name="services">
        /// A service provider from which to host services.
        /// </param>
        /// <returns>
        /// A configured operator.
        /// </returns>
        public static ChildOperatorBuilder<WebDriverSession, V1Ingress> BuildIngressOperator(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var kubernetes = services.GetRequiredService<KubernetesClient>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FakeOperator");

            return new ChildOperatorBuilder(services)
                .CreateOperator("WebDriverSession-IngressOperator")
                .Watches<WebDriverSession>()
                .Where(s => s.Status?.SessionId != null)
                .Creates<V1Ingress>(
                (session, ingress) =>
                {
                    // On the public endpoint, the session id will be the name assigned to the WebDriverSession object.
                    // This is usually a name auto-generated by Kubernetes.
                    // Within the pod running the WebDriver session, the session id will be different - typically a Guid.
                    // Use HTTP request rewriting to patch the session name in the WebDriver URLs.
                    //
                    // URL rewriting is not yet fully standardized within Kubernetes, and this adds a hard dependency on
                    // Traefik as the reverse proxy.
                    //
                    // Traefik was chosen because it's the default reverse proxy in k3s. Adding support for nginx should
                    // not be particularly hard.
                    ingress.EnsureMetadata().EnsureAnnotations();

                    ingress.Metadata.Annotations.Add(Annotations.RequestModifier, $"ReplacePathRegex: /wd/hub/session/{session.Metadata.Name}/(.*) /wd/hub/session/{session.Status.SessionId}/$1");
                    ingress.Metadata.Annotations.Add(Annotations.IngressClass, "traefik");

                    // No need to validate Status.SessionId != null, that's handled by the Where clause above.
                    ingress.Spec = new V1IngressSpec()
                    {
                        Rules = new V1IngressRule[]
                        {
                            new V1IngressRule()
                            {
                                Http = new V1HTTPIngressRuleValue()
                                {
                                    Paths = new V1HTTPIngressPath[]
                                    {
                                        new V1HTTPIngressPath()
                                        {
                                            Path = $"/wd/hub/session/{session.Metadata.Name}/",
                                            PathType = "Prefix",
                                            Backend = new V1IngressBackend()
                                            {
                                                Service = new V1IngressServiceBackend()
                                                {
                                                    Name = session.Metadata.Name,
                                                    Port = new V1ServiceBackendPort()
                                                    {
                                                        Number = session.Status.SessionPort,
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    };
                })
                .PostsFeedback((context, cancellationToken) =>
                {
                    Feedback<WebDriverSession, V1Ingress> feedback = null;

                    var session = context.Parent;
                    var ingress = context.Child;

                    if (ingress?.Status?.LoadBalancer?.Ingress == null
                        || ingress.Status.LoadBalancer.Ingress.Count == 0)
                    {
                        logger.LogInformation("Not setting the ingress status to ready for session {session} because the ingress does not have any load balancer endpoints.", context.Parent?.Metadata?.Name);
                    }
                    else if (!session.Status.IngressReady)
                    {
                        feedback = new Feedback<WebDriverSession, V1Ingress>();
                        feedback.ParentFeedback = new JsonPatchDocument<WebDriverSession>();
                        feedback.ParentFeedback.Add(s => s.Status.IngressReady, true);
                    }

                    return Task.FromResult(feedback);
                });
        }

        /// <summary>
        /// Builds an operator which provisions services for <see cref="WebDriverSession"/> objects which
        /// use the Fake driver.
        /// </summary>
        /// <param name="services">
        /// A service provider from which to host services.
        /// </param>
        /// <returns>
        /// A configured operator.
        /// </returns>
        public static ChildOperatorBuilder<WebDriverSession, V1Service> BuildServiceOperator(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var kubernetes = services.GetRequiredService<KubernetesClient>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("WebDriverSession-ServiceOperator");

            return new ChildOperatorBuilder(services)
                .CreateOperator("WebDriverSession-ServiceOperator")
                .Watches<WebDriverSession>()
                .Where(s => s.Status?.SessionId != null)
                .Creates<V1Service>(
                (session, service) =>
                {
                    service.Spec = new V1ServiceSpec()
                    {
                        Selector = new Dictionary<string, string>()
                        {
                            { Annotations.SessionName, session.Metadata.Name },
                        },
                        Ports = new V1ServicePort[]
                        {
                            new V1ServicePort()
                            {
                                 Protocol = "TCP",
                                 Port = session.Status.SessionPort,
                                 TargetPort = session.Status.SessionPort,
                            },
                        },
                    };
                })
                .PostsFeedback((context, cancellationToken) =>
                {
                    Feedback<WebDriverSession, V1Service> feedback = null;

                    var session = context.Parent;
                    var service = context.Child;

                    if (service != null && !session.Status.ServiceReady)
                    {
                        feedback = new Feedback<WebDriverSession, V1Service>()
                        {
                            ParentFeedback = new JsonPatchDocument<WebDriverSession>(),
                        };

                        feedback.ParentFeedback.Add(s => s.Status.ServiceReady, true);
                    }

                    return Task.FromResult(feedback);
                });
        }
    }
}