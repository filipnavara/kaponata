// <copyright file="ChildOperatorBuilder.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using System;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Builds <see cref="ChildOperator{TParent, TChild}"/> objects.
    /// </summary>
    public class ChildOperatorBuilder
    {
        private readonly IServiceProvider services;
        private ChildOperatorConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorBuilder"/> class.
        /// </summary>
        /// <param name="services">
        /// A host from which required services can be sourced.
        /// </param>
        public ChildOperatorBuilder(IServiceProvider services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Configures the builder to create a new operator.
        /// </summary>
        /// <param name="operatorName">
        /// The name of the operator to create.
        /// </param>
        /// <returns>
        /// A builder which can be used to futher configure the operator.
        /// </returns>
        public ChildOperatorBuilder CreateOperator(string operatorName)
        {
            this.configuration = new ChildOperatorConfiguration(operatorName);
            return this;
        }

        /// <summary>
        /// Configures the operator to watch objects of a given type.
        /// </summary>
        /// <typeparam name="TParent">
        /// The type of parent objects to watch.
        /// </typeparam>
        /// <returns>
        /// A builder which can be used to futher configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent> Watches<TParent>()
            where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
        {
            return new ChildOperatorBuilder<TParent>(
                this.services,
                this.configuration);
        }
    }
}
