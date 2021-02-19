// <copyright file="ChildOperatorContext.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Microsoft.Extensions.Logging;
using System;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// The <see cref="ChildOperatorContext{TParent, TChild}"/> represents a specific instance of objects (parent + child)
    /// which need to be reconciled.
    /// </summary>
    /// <typeparam name="TParent">
    /// The type of object to watch for (such as <see cref="V1Pod"/>).
    /// </typeparam>
    /// <typeparam name="TChild">
    /// The type of object to create (such as <see cref="V1Service"/>).
    /// </typeparam>
    public struct ChildOperatorContext<TParent, TChild>
        where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
        where TChild : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorContext{TParent, TChild}"/> struct.
        /// </summary>
        /// <param name="parent">
        /// The parent object being reconciled.
        /// </param>
        /// <param name="child">
        /// The child object being reconciled.
        /// </param>
        /// <param name="kubernetes">
        /// A <see cref="KubernetesClient"/> which provides access to the Kubernetes API.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging diagnostic messages.
        /// </param>
        public ChildOperatorContext(TParent parent, TChild child, KubernetesClient kubernetes, ILogger logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.Child = child;
        }

        /// <summary>
        /// Gets a <see cref="ILogger"/> which can be used when logging diagnostic messages.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets a <see cref="KubernetesClient"/> which provides access to the Kubernetes API.
        /// </summary>
        public KubernetesClient Kubernetes { get; }

        /// <summary>
        /// Gets the parent for which the reconciliation is being executed.
        /// </summary>
        public TParent Parent { get; }

        /// <summary>
        /// Gets the child for which the reconciliation is being executed. Can be <see langword="null"/>
        /// if no child exists.
        /// </summary>
        public TChild Child { get; }
    }
}
