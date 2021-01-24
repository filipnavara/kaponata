// <copyright file="KubernetesClient.Crd.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Rest;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Contains methods for working with Custom Resource Definitions.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <summary>
        /// Asynchronously creates a new custom resource definition.
        /// </summary>
        /// <param name="value">
        /// The custom resource definition to create.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the newly created custom resource definition
        /// when completed.
        /// </returns>
        public virtual Task<V1CustomResourceDefinition> CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition value, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return this.RunTaskAsync(this.protocol.CreateCustomResourceDefinitionAsync(value, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Asynchronously tries to read a custom resource definition.
        /// </summary>
        /// <param name="name">
        /// The name which uniquely identifies the custom resource definition.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns the requested custom resource definition., or
        /// <see langword="null"/> if the custom resource definition does not exist.
        /// </returns>
        public virtual async Task<V1CustomResourceDefinition> TryReadCustomResourceDefinitionAsync(string name, CancellationToken cancellationToken)
        {
            var list = await this.RunTaskAsync(this.protocol.ListCustomResourceDefinitionAsync(fieldSelector: $"metadata.name={name}", cancellationToken: cancellationToken)).ConfigureAwait(false);
            return list.Items.SingleOrDefault();
        }

        /// <summary>
        /// Asynchronously deletes a custom resource definition.
        /// </summary>
        /// <param name="crd">
        /// The custom resource definition to delete.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the pod should be deleted.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task DeleteCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return this.DeleteObjectAsync<V1CustomResourceDefinition>(
                crd,
                this.protocol.DeleteCustomResourceDefinitionAsync,
                this.protocol.WatchCustomResourceDefinitionAsync,
                timeout,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously installs a <see cref="V1CustomResourceDefinition"/> in the cluster, or, if the <see cref="V1CustomResourceDefinition"/>
        /// is already installed, upgrades the <see cref="V1CustomResourceDefinition"/> if required.
        /// </summary>
        /// <param name="crd">
        /// The custom resource definition to install into the cluster.
        /// </param>
        /// <param name="timeout">
        /// The amount of time alotted to the operation.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<V1CustomResourceDefinition> InstallOrUpgradeCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (crd == null)
            {
                throw new ArgumentNullException(nameof(crd));
            }

            if (crd.Metadata?.Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Metadata.Name");
            }

            if (crd.Metadata?.Labels == null || !crd.Metadata.Labels.ContainsKey(Annotations.Version))
            {
                throw new ValidationException(ValidationRules.CannotBeNull, $"value.Metadata.Labels[{Annotations.Version}]");
            }

            var installedCrd = await this.TryReadCustomResourceDefinitionAsync(
                crd.Metadata.Name,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (installedCrd == null)
            {
                // Deploy the CRD
                return await this.CreateCustomResourceDefinitionAsync(
                    crd,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var version = new Version(crd.Metadata.Labels[Annotations.Version]);

                // Get the version of the installed CRD; set the version to 0.0 if not version
                // annotation could be found.
                var installedVersion = new Version(0, 0);
                if (installedCrd.Metadata.Labels.ContainsKey(Annotations.Version))
                {
                    installedVersion = new Version(installedCrd.Metadata.Labels[Annotations.Version]);
                }

                if (version > installedVersion)
                {
                    // Upgrade: delete and reinstall
                    await this.DeleteCustomResourceDefinitionAsync(
                        installedCrd,
                        timeout,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    return await this.CreateCustomResourceDefinitionAsync(
                        crd,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return installedCrd;
                }
            }
        }

        /// <summary>
        /// Asynchronously waits for a CRD to enter be established.
        /// </summary>
        /// <param name="value">
        /// The CRD which should be established.
        /// </param>
        /// <param name="timeout">
        /// The amount of time in which the CRD should be established.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The <see cref="V1CustomResourceDefinition"/>.
        /// </returns>
        public virtual async Task<V1CustomResourceDefinition> WaitForCustomResourceDefinitionEstablishedAsync(V1CustomResourceDefinition value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (IsEstablished(value))
            {
                return value;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationToken.Register(cts.Cancel);

            var watchTask = this.protocol.WatchCustomResourceDefinitionAsync(
                value,
                eventHandler: (type, updatedCrd) =>
                {
                    value = updatedCrd;

                    if (type == WatchEventType.Deleted)
                    {
                        throw new KubernetesException($"The CRD '{value.Metadata.Name}' was deleted.");
                    }

                    if (IsEstablished(updatedCrd))
                    {
                        return Task.FromResult(WatchResult.Stop);
                    }

                    return Task.FromResult(WatchResult.Continue);
                },
                cts.Token);

            if (await Task.WhenAny(watchTask, Task.Delay(timeout)).ConfigureAwait(false) != watchTask)
            {
                cts.Cancel();
                throw new KubernetesException($"The CRD '{value.Metadata.Name}' was not established within a timeout of {timeout.TotalSeconds} seconds.");
            }

            var result = await watchTask.ConfigureAwait(false);

            if (result != WatchExitReason.ClientDisconnected)
            {
                throw new KubernetesException($"The API server unexpectedly closed the connection while watching CRD '{value.Metadata.Name}'.");
            }

            return value;
        }

        private static bool IsEstablished(V1CustomResourceDefinition value)
        {
            if (value?.Status?.Conditions == null)
            {
                return false;
            }

            return value.Status.Conditions.Any(
                c => string.Equals(c.Type, "Established", StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.Status, "True", StringComparison.OrdinalIgnoreCase));
        }
    }
}
