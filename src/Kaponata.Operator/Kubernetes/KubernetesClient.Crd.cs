// <copyright file="KubernetesClient.Crd.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
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
    }
}
