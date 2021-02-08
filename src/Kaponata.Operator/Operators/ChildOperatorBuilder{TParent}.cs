// <copyright file="ChildOperatorBuilder{TParent}.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq.Expressions;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Builds <see cref="ChildOperator{TParent, TChild}"/> objects.
    /// </summary>
    /// <typeparam name="TParent">
    /// The parent object used by the <see cref="ChildOperator{TParent, TChild}"/>.
    /// </typeparam>
    public class ChildOperatorBuilder<TParent>
            where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        private readonly IHost host;
        private readonly ChildOperatorConfiguration configuration;
        private Func<TParent, bool> parentFilter = (parent) => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorBuilder{TParent}"/> class.
        /// </summary>
        /// <param name="host">
        /// The host from which to source required services.
        /// </param>
        /// <param name="configuration">
        /// The configuration to apply to the operator.
        /// </param>
        public ChildOperatorBuilder(IHost host, ChildOperatorConfiguration configuration)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Configures the operator to only consider parent items which have certain labels applied to them.
        /// </summary>
        /// <param name="labelSelector">
        /// An expression which represents the label selector.
        /// </param>
        /// <returns>
        /// A builder which can be used to further configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent> WithLabels(
            Expression<Func<TParent, bool>> labelSelector)
        {
            return this.WithLabels(LabelSelector.Create(labelSelector));
        }

        /// <summary>
        /// Configures the operator to only consider parent items which have certain labels applied to them.
        /// </summary>
        /// <param name="labelSelector">
        /// The label selector.
        /// </param>
        /// <returns>
        /// A builder which can be used to further configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent> WithLabels(
            string labelSelector)
        {
            this.configuration.ParentLabelSelector = labelSelector;

            return this;
        }

        /// <summary>
        /// Configures the operator to filter parent object using a function.
        /// </summary>
        /// <param name="parentFilter">
        /// A function which defines which parent object should be considered by the operator, and which not.
        /// </param>
        /// <returns>
        /// A builder which can be used to further configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent> Where(Func<TParent, bool> parentFilter)
        {
            this.parentFilter = parentFilter ?? throw new ArgumentNullException(nameof(parentFilter));
            return this;
        }

        /// <summary>
        /// Configures the operator to create child objects of a given type.
        /// </summary>
        /// <typeparam name="TChild">
        /// The type of objects to create.
        /// </typeparam>
        /// <param name="childFactory">
        /// An action which can be used to configure the newly created child objects.
        /// </param>
        /// <returns>
        /// A builder which can be used to further configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent, TChild> Creates<TChild>(
            Action<TParent, TChild> childFactory)
            where TChild : class, IKubernetesObject<V1ObjectMeta>, new()
        {
            return new ChildOperatorBuilder<TParent, TChild>(this.host, this.configuration, this.parentFilter, childFactory);
        }
    }
}
