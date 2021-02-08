// <copyright file="KubernetesServiceCollectionExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Extension methods for setting up Kubernetes services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class KubernetesServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="KubernetesClient"/> to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add services to.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddKubernetes(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildDefaultConfig());
            services.AddSingleton<IKubernetesProtocol, KubernetesProtocol>();
            services.AddSingleton<KubernetesClient>();
            return services;
        }
    }
}
