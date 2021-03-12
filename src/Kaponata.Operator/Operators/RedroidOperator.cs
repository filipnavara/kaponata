// <copyright file="RedroidOperator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// The <see cref="RedroidOperator"/> detects Android running in Docker containers and creates <see cref="MobileDevice"/>
    /// objects.
    /// </summary>
    public class RedroidOperator
    {
        /// <summary>
        /// Creates a builder which can build the redroid operator.
        /// </summary>
        /// <param name="services">
        /// The service provider from which to source the required services.
        /// </param>
        /// <returns>
        /// A builder which can build the redroid operator.
        /// </returns>
        public static ChildOperatorBuilder<V1Pod, MobileDevice> BuildRedroidOperator(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var kubernetes = services.GetRequiredService<KubernetesClient>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("RedroidOperator");

            return new ChildOperatorBuilder(services)
                .CreateOperator("RedroidOperator")
                .Watches<V1Pod>()
                .WithLabels(s => s.Metadata.Labels[Annotations.Os] == Annotations.OperatingSystem.Android)
                .Where(pod => pod.Status?.ContainerStatuses != null && pod.Status.ContainerStatuses.All(c => c.Ready))
                .Creates<MobileDevice>(
                    (pod, device) =>
                    {
                        device.EnsureMetadata().EnsureLabels();
                        device.Metadata.Labels.Add(Annotations.Os, Annotations.OperatingSystem.Android);

                        device.Spec = new MobileDeviceSpec()
                        {
                            Owner = pod.Metadata.Name,
                        };
                    })
               .PostsFeedback((context, cancellationToken) =>
               {
                   var pod = context.Parent;
                   var device = context.Child;

                   Feedback<V1Pod, MobileDevice> feedback = new Feedback<V1Pod, MobileDevice>();

                   if (device.Status?.VncHost != pod.Status?.PodIP)
                   {
                       feedback.ChildFeedback = new JsonPatchDocument<MobileDevice>();

                       if (device.Status == null)
                       {
                           feedback.ChildFeedback.Add(d => d.Status, new MobileDeviceStatus() { VncHost = pod.Status?.PodIP });
                       }
                       else
                       {
                           feedback.ChildFeedback.Add(d => d.Status.VncHost, pod.Status?.PodIP);
                       }
                   }

                   return Task.FromResult(feedback);
               });
        }
    }
}
