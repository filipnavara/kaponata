// <copyright file="ExtensionsInstaller.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator
{
    /// <summary>
    /// Installs the Kaponata extensions, which cannot be installed using a standard Helm chart, into your cluster.
    /// </summary>
    public class ExtensionsInstaller
    {
        private readonly KubernetesClient kubernetesClient;
        private readonly ILogger<ExtensionsInstaller> logger;
        private readonly IConsole console;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionsInstaller"/> class.
        /// </summary>
        /// <param name="kubernetesClient">
        /// A <see cref="KubernetesClient"/> which provides connectivity to the Kubernetes cluster.
        /// </param>
        /// <param name="logger">
        /// A logger to use when logging.
        /// </param>
        /// <param name="console">
        /// The console to use when printing output.
        /// </param>
        public ExtensionsInstaller(KubernetesClient kubernetesClient, ILogger<ExtensionsInstaller> logger, IConsole console)
        {
            this.kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.console = console ?? throw new ArgumentNullException(nameof(console));
        }

        /// <summary>
        /// Asynchronously runs the <see cref="ExtensionsInstaller.InstallAsync(CancellationToken)"/> method.
        /// </summary>
        /// <param name="host">
        /// A <see cref="IHost"/> which contains all required services.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public static Task RunAsync(IHost host)
        {
            var installer = host.Services.GetRequiredService<ExtensionsInstaller>();
            return installer.InstallAsync(default);
        }

        /// <summary>
        /// Asynchronously installs the Kaponata extensions into the cluster.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task InstallAsync(CancellationToken cancellationToken)
        {
            await this.InstallCrdAsync(ModelDefinitions.MobileDevice, cancellationToken).ConfigureAwait(false);
            await this.InstallCrdAsync(ModelDefinitions.WebDriverSession, cancellationToken).ConfigureAwait(false);
        }

        private async Task InstallCrdAsync(V1CustomResourceDefinition crd, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Installing the {crdName} custom resource definition", crd.Metadata.Name);
            await this.kubernetesClient.InstallOrUpgradeCustomResourceDefinitionAsync(
                crd,
                TimeSpan.FromMinutes(1),
                cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation("Successfully installed the {crdName} custom resource definition.", crd.Metadata.Name);
        }
    }
}
