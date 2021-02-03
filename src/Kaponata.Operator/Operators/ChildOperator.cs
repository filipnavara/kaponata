// <copyright file="ChildOperator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// A <see cref="ChildOperator{TParent, TChild}"/> watches for objects of type <typeparamref name="TParent"/>
    /// and projects them into objects of type <typeparamref name="TChild"/>.
    /// </summary>
    /// <typeparam name="TParent">
    /// The type of object to watch for (such as <see cref="V1Pod"/>).
    /// </typeparam>
    /// <typeparam name="TChild">
    /// The type of object to create (such as <see cref="V1Service"/>).
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Only a subset of objects of type <typeparamref name="TParent"/> are watched. Use the <see cref="ChildOperatorConfiguration.ParentLabelSelector"/>
    /// property to limit which <typeparamref name="TParent"/> objects are being considered by this operator.
    /// </para>
    /// <para>
    /// This operator will pre-configure the child objects as follows:
    /// <list type="bullet">
    ///   <item>
    ///     Add an entry in the  <see cref="V1ObjectMeta.OwnerReferences"/> table which references the parent object,
    ///     causing it to be garbage-collected when the parent object has been deleted.
    ///   </item>
    ///   <item>
    ///     Set the <see cref="V1ObjectMeta.Name"/> and <see cref="V1ObjectMeta.NamespaceProperty"/> values to match
    ///     that of the parent object.
    ///   </item>
    ///   <item>
    ///     Populate the <see cref="V1ObjectMeta.Labels"/> property with the value of the <see cref="ChildOperatorConfiguration.ChildLabels"/>
    ///     property, and add a <see cref="Annotations.ManagedBy"/> label which matches the <see cref="ChildOperatorConfiguration.OperatorName"/>.
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// This operator assumes that:
    /// <list type="bullet">
    ///   <item>
    ///     The child and parent have the same <see cref="V1ObjectMeta.Name"/> and <see cref="V1ObjectMeta.NamespaceProperty"/> values,
    ///     and these values are sufficient to correlate child with parent and vice versa.
    ///   </item>
    ///   <item>
    ///     All parent objects have the <see cref="ChildOperatorConfiguration.ParentLabelSelector"/> and all child objects have the
    ///     <see cref="ChildOperatorConfiguration.ChildLabels"/> labels.
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ChildOperator<TParent, TChild> : BackgroundService
        where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
        where TChild : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        private readonly ILogger<ChildOperator<TParent, TChild>> logger;
        private readonly ChildOperatorConfiguration configuration;
        private readonly KubernetesClient kubernetes;
        private readonly Action<TParent, TChild> childFactory;

        // Kubernetes clients which are used to watch and upated parent and child objects.
        private readonly NamespacedKubernetesClient<TParent> parentClient;
        private readonly NamespacedKubernetesClient<TChild> childClient;

        // The queue to which items to reconcile are posted and from which they are read.
        private readonly BufferBlock<ChildOperatorContext> reconciliationBuffer = new BufferBlock<ChildOperatorContext>();

        // Task completion source backing the InitializationCompleted property.
        private readonly TaskCompletionSource initializationCompletedTcs = new TaskCompletionSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperator{TParent, TChild}"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A connection to a Kubernetes cluster.
        /// </param>
        /// <param name="configuration">
        /// The configuration for this operator.
        /// </param>
        /// <param name="factory">
        /// A method which projects objects of type <typeparamref name="TParent"/> into objects of type
        /// <typeparamref name="TChild"/>.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public ChildOperator(
            KubernetesClient kubernetes,
            ChildOperatorConfiguration configuration,
            Action<TParent, TChild> factory,
            ILogger<ChildOperator<TParent, TChild>> logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.childFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.parentClient = this.kubernetes.GetClient<TParent>();
            this.childClient = this.kubernetes.GetClient<TChild>();
        }

        /// <summary>
        /// Gets a <see cref="BufferBlock{T}"/> which contains all currently scheduled reconciliations.
        /// </summary>
        public BufferBlock<ChildOperatorContext> ReconcilationBuffer => this.reconciliationBuffer;

        /// <summary>
        /// Gets a <see cref="Task"/> which completes when the initialization has completed.
        /// </summary>
        public Task InitializationCompleted => this.initializationCompletedTcs.Task;

        /// <summary>
        /// Runs the initial reconcilation loop. Lists all parents and schedules the initial reconcilation for
        /// these objects.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Initializing the {operator} operator.", this.configuration.OperatorName);

                // List all parent and child objects
                var parents = await this.parentClient.ListAsync(
                    this.configuration.Namespace,
                    fieldSelector: null,
                    labelSelector: this.configuration.ParentLabelSelector,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Found {count} parents for the {operator} operator.", parents.Items.Count, this.configuration.OperatorName);

                var children = await this.childClient.ListAsync(
                    this.configuration.Namespace,
                    labelSelector: Selector.Create(this.configuration.ChildLabels)).ConfigureAwait(false);
                this.logger.LogInformation("Found {count} children for the {operator} operator.", children.Items.Count, this.configuration.OperatorName);

                foreach (var parent in parents.Items)
                {
                    this.logger.LogInformation("Processing parent {parent} for the {operator} operator", parent.Metadata.Name, this.configuration.OperatorName);

                    var child = children.Items.SingleOrDefault(c => c.IsOwnedBy(parent));
                    this.logger.LogInformation("Found child {child} for parent {parent} for the {operator} operator", child?.Metadata?.Name, parent.Metadata.Name, this.configuration.OperatorName);

                    this.reconciliationBuffer.Post(new ChildOperatorContext(parent, child));

                    if (child != null)
                    {
                        children.Items.Remove(child);
                    }

                    this.logger.LogInformation("{parentCount} parents left, {childCount} chidren left for the {operator} operator.", parents.Items.Count, children.Items.Count, this.configuration.OperatorName);
                }

                // We don't care for children without parents; these should be child objects which are being deleted
                // because of cascading background deletions.
                this.initializationCompletedTcs.SetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while scheduling initializing operator {operator}", ex.Message, this.configuration.OperatorName);
                this.initializationCompletedTcs.SetException(ex);
            }
        }

        /// <summary>
        /// Schedules a reconciliation based on a parent object, but does not wait for the
        /// reconciliation to actually be performed.
        /// </summary>
        /// <param name="parent">
        /// A parent object, which has been modified.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation of scheduling
        /// the reconciliation.
        /// </returns>
        public async Task ScheduleReconciliationAsync(TParent parent, CancellationToken cancellationToken)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            try
            {
                this.logger.LogInformation("{operator} operator: scheduling reconciliation for parent {parent}", this.configuration.OperatorName, parent?.Metadata?.Name);

                // Get the child
                var children = await this.childClient.ListAsync(
                    this.configuration.Namespace,
                    null,
                    $"metadata.name={parent.Metadata.Name}",
                    Selector.Create(this.configuration.ChildLabels),
                    null,
                    cancellationToken).ConfigureAwait(false);
                var child = children.Items.SingleOrDefault();

                this.logger.LogInformation("{operator} operator: found child {child} for parent {parent}", this.configuration.OperatorName, child?.Metadata?.Name, parent?.Metadata?.Name);

                this.reconciliationBuffer.Post(new ChildOperatorContext(parent, child));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while scheduling parent reconciliation for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
        }

        /// <summary>
        /// Schedules a reconcilation based on a child object, but does not wait for
        /// the reconciliation to be actually performed.
        /// </summary>
        /// <param name="child">
        /// A child object, which has been modified.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task ScheduleReconciliationAsync(TChild child, CancellationToken cancellationToken)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            try
            {
                this.logger.LogInformation("{operator} operator: scheduling reconciliation for child {parent}", this.configuration.OperatorName, child?.Metadata?.Name);

                // Get the parent
                var parents = await this.parentClient.ListAsync(
                    this.configuration.Namespace,
                    null,
                    $"metadata.name={child.Metadata.Name}",
                    this.configuration.ParentLabelSelector,
                    null,
                    cancellationToken);
                var parent = parents.Items.SingleOrDefault();

                this.logger.LogInformation("{operator} operator: found parent {parent} for child {child}", this.configuration.OperatorName, parent?.Metadata?.Name, child?.Metadata?.Name);

                if (parent != null)
                {
                    this.reconciliationBuffer.Post(new ChildOperatorContext(parent, child));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while scheduling child reconciliation for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
        }

        /// <summary>
        /// Executes the queued reconciliations.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task ExecuteReconcilationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await this.reconciliationBuffer.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                    await this.ReconcileAsync(context, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while executing reconciliations for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
        }

        /// <summary>
        /// Attempts to reconcile a child and parent object.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ChildOperatorContext"/> object which represents the parent and child object
        /// being reconciled.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task ReconcileAsync(ChildOperatorContext context, CancellationToken cancellationToken)
        {
            // Let's assume Kubernetes takes care of garbage collection, so the source
            // object is always present.
            //
            // Although children with no parents are possible because of cascading background deletions:
            // https://kubernetes.io/docs/concepts/workloads/controllers/garbage-collection/#background-cascading-deletion,
            // they should never enter the reconcile loop.
            Debug.Assert(context.Parent != null, "Cannot have a child object without a parent object");

            try
            {
                this.logger.LogInformation(
                    "Scheduling reconciliation for parent {parent} and child {child} for operator {operatorName}",
                    context.Parent?.Metadata?.Name,
                    context.Child?.Metadata?.Name,
                    this.configuration.OperatorName);

                if (context.Child == null)
                {
                    // Create a new object
                    var child = new TChild()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Labels = this.configuration.ChildLabels,
                            Name = context.Parent.Metadata.Name,
                            NamespaceProperty = context.Parent.Metadata.NamespaceProperty,
                            OwnerReferences = new V1OwnerReference[]
                            {
                            new V1OwnerReference()
                            {
                                Kind = context.Parent.Kind,
                                ApiVersion = context.Parent.ApiVersion,
                                BlockOwnerDeletion = false,
                                Controller = false,
                                Name = context.Parent.Metadata.Name,
                                Uid = context.Parent.Metadata.Uid,
                            },
                            },
                        },
                    };

                    child.SetLabel(Annotations.ManagedBy, this.configuration.OperatorName);

                    this.childFactory(context.Parent, child);

                    await this.childClient.CreateAsync(child, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    this.logger.LogInformation(
                        "Skipping reconciliation for parent {parent} and child {child} for operator {operatorName} because child is null",
                        context.Parent?.Metadata?.Name,
                        context.Child?.Metadata?.Name,
                        this.configuration.OperatorName);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while executing reconciliation for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Watch child objects
            var sourceWatch = this.parentClient.WatchAsync(
                this.configuration.Namespace,
                fieldSelector: null,
                this.configuration.ParentLabelSelector,
                null,
                async (eventType, value) =>
                {
                    await this.ScheduleReconciliationAsync(value, stoppingToken).ConfigureAwait(false);
                    return WatchResult.Continue;
                },
                stoppingToken);

            var targetWatch = this.childClient.WatchAsync(
                this.configuration.Namespace,
                fieldSelector: null,
                Selector.Create(this.configuration.ChildLabels),
                null,
                async (eventType, value) =>
                {
                    await this.ScheduleReconciliationAsync(value, stoppingToken).ConfigureAwait(false);
                    return WatchResult.Continue;
                },
                stoppingToken);

            // Schedule the initial reconciliations
            await this.InitializeAsync(stoppingToken).ConfigureAwait(false);

            var reconcilerTask = this.ExecuteReconcilationsAsync(stoppingToken);

            await Task.WhenAny(
                sourceWatch,
                targetWatch,
                reconcilerTask).ConfigureAwait(false);
        }

        /// <summary>
        /// The <see cref="ChildOperatorContext"/> represents a specific instance of objects (parent + child)
        /// which need to be reconciled.
        /// </summary>
        public class ChildOperatorContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChildOperatorContext"/> class.
            /// </summary>
            /// <param name="parent">
            /// The parent object being reconciled.
            /// </param>
            /// <param name="child">
            /// The child object being reconciled.
            /// </param>
            public ChildOperatorContext(TParent parent, TChild child)
            {
                this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.Child = child;
            }

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
}
