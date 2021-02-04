// <copyright file="ChildOperatorBuilder{TParent,TChild}.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Builds <see cref="ChildOperator{TParent, TChild}"/> objects.
    /// </summary>
    /// <typeparam name="TParent">
    /// The parent object used by the <see cref="ChildOperator{TParent, TChild}"/>.
    /// </typeparam>
    /// <typeparam name="TChild">
    /// The child object used by the <see cref="ChildOperator{TParent, TChild}"/>.
    /// </typeparam>
    public class ChildOperatorBuilder<TParent, TChild>
            where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
            where TChild : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        private readonly ChildOperatorConfiguration configuration;
        private readonly Action<TParent, TChild> childFactory;
        private readonly Collection<FeedbackLoop<TParent, TChild>> feedbackLoops = new ();
        private readonly IHost host;
        private Func<TParent, bool> parentFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorBuilder{TParent, TChild}"/> class.
        /// </summary>
        /// <param name="host">
        /// The host from which to source required services.
        /// </param>
        /// <param name="configuration">
        /// The configuration for the operator.
        /// </param>
        /// <param name="parentFilter">
        /// A parent filter which defines which parent objects are considered, and which not.
        /// </param>
        /// <param name="childFactory">
        /// A method which projects objects of type <typeparamref name="TParent"/> into objects of type
        /// <typeparamref name="TChild"/>.
        /// </param>
        public ChildOperatorBuilder(IHost host, ChildOperatorConfiguration configuration, Func<TParent, bool> parentFilter, Action<TParent, TChild> childFactory)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.parentFilter = parentFilter ?? throw new ArgumentNullException(nameof(parentFilter));
            this.childFactory = childFactory ?? throw new ArgumentNullException(nameof(childFactory));
        }

        /// <summary>
        /// Adds a feedback loop to the operator.
        /// </summary>
        /// <param name="feedbackLoop">
        /// The feedback loop to add to the operator.
        /// </param>
        /// <returns>
        /// An operator builder which can be used to further configure the operator.
        /// </returns>
        public ChildOperatorBuilder<TParent, TChild> PostsFeedback(
            FeedbackLoop<TParent, TChild> feedbackLoop)
        {
            if (feedbackLoop == null)
            {
                throw new ArgumentNullException(nameof(feedbackLoop));
            }

            this.feedbackLoops.Add(feedbackLoop);

            return this;
        }

        /// <summary>
        /// Builds an operator.
        /// </summary>
        /// <returns>
        /// A configured <see cref="ChildOperator{TParent, TChild}"/> object.
        /// </returns>
        public ChildOperator<TParent, TChild> Build()
        {
            var kubernetesClient = this.host.Services.GetRequiredService<KubernetesClient>();
            var loggerFactory = this.host.Services.GetRequiredService<ILoggerFactory>();

            return new ChildOperator<TParent, TChild>(
                kubernetesClient,
                this.configuration,
                this.parentFilter,
                this.childFactory,
                this.feedbackLoops,
                loggerFactory.CreateLogger<ChildOperator<TParent, TChild>>());
        }
    }
}
