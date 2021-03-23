// <copyright file="KubernetesServiceCollectionExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using Kaponata.Kubernetes.DeveloperProfiles;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kaponata.Kubernetes
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
        /// <param name="namespace">
        /// The namespace in which to operate. The default value is <c>default</c>.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddKubernetes(this IServiceCollection services, string @namespace = "default")
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildDefaultConfig());
            services.AddSingleton<IKubernetesProtocol, KubernetesProtocol>();
            services.AddSingleton<KubernetesClient>();

            services.AddScoped<KubernetesDeveloperProfile>();

            services.AddOptions<KubernetesOptions>().Configure(c => c.Namespace = @namespace);
            return services;
        }
    }
}
