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
using System.Collections.Generic;
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
        private readonly Func<TParent, bool> parentFilter;
        private readonly Action<TParent, TChild> childFactory;
        private readonly Collection<FeedbackLoop<TParent, TChild>> feedbackLoops;

        // Kubernetes clients which are used to watch and upated parent and child objects.
        private readonly NamespacedKubernetesClient<TParent> parentClient;
        private readonly NamespacedKubernetesClient<TChild> childClient;

        // The queue to which items to reconcile are posted and from which they are read.
        private readonly BufferBlock<string> reconciliationBuffer = new BufferBlock<string>();

        // Task completion source backing the InitializationCompleted property.
        private readonly TaskCompletionSource initializationCompletedTcs = new TaskCompletionSource();

        // A semaphore used to protect the ReconcileAsync method; only one reconciliation can happen at a time.
        private readonly SemaphoreSlim reconciliationSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim processingSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperator{TParent, TChild}"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A connection to a Kubernetes cluster.
        /// </param>
        /// <param name="configuration">
        /// The configuration for this operator.
        /// </param>
        /// <param name="parentFilter">
        /// A delegate which can be used to only take a subset of the parents in consideration.
        /// </param>
        /// <param name="childFactory">
        /// A method which projects objects of type <typeparamref name="TParent"/> into objects of type
        /// <typeparamref name="TChild"/>.
        /// </param>
        /// <param name="feedbackLoops">
        /// A list of feedback loops which can update the state of the parent object depending
        /// on the state of the child object.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public ChildOperator(
            KubernetesClient kubernetes,
            ChildOperatorConfiguration configuration,
            Func<TParent, bool> parentFilter,
            Action<TParent, TChild> childFactory,
            Collection<FeedbackLoop<TParent, TChild>> feedbackLoops,
            ILogger<ChildOperator<TParent, TChild>> logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.parentFilter = parentFilter ?? throw new ArgumentNullException(nameof(parentFilter));
            this.childFactory = childFactory ?? throw new ArgumentNullException(nameof(childFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.feedbackLoops = feedbackLoops ?? throw new ArgumentNullException(nameof(feedbackLoops));

            this.parentClient = this.kubernetes.GetClient<TParent>();
            this.childClient = this.kubernetes.GetClient<TChild>();
        }

        /// <summary>
        /// Gets a <see cref="BufferBlock{T}"/> which contains all currently scheduled reconciliations.
        /// </summary>
        public BufferBlock<string> ReconcilationBuffer => this.reconciliationBuffer;

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

                    if (!this.parentFilter(parent))
                    {
                        this.logger.LogInformation("Skipping parent {parent} because it is filtered out", parent.Metadata.Name, this.configuration.OperatorName);
                        continue;
                    }

                    var child = children.Items.SingleOrDefault(c => c.IsOwnedBy(parent));
                    this.logger.LogInformation("Found child {child} for parent {parent} for the {operator} operator", child?.Metadata?.Name, parent.Metadata.Name, this.configuration.OperatorName);

                    this.reconciliationBuffer.Post(parent.Metadata.Name);

                    if (child != null)
                    {
                        children.Items.Remove(child);
                    }

                    this.logger.LogInformation("{parentCount} parents left, {childCount} chidren left for the {operator} operator.", parents.Items.Count, children.Items.Count, this.configuration.OperatorName);
                }

                // We don't care for children without parents; these should be child objects which are being deleted
                // because of cascading background deletions.
                this.initializationCompletedTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while scheduling initializing operator {operator}", ex.Message, this.configuration.OperatorName);
                this.initializationCompletedTcs.TrySetException(ex);
            }
        }

        /// <summary>
        /// Schedules a reconciliation based on a parent object, but does not wait for the
        /// reconciliation to actually be performed.
        /// </summary>
        /// <param name="parent">
        /// A parent object, which has been modified.
        /// </param>
        public void ScheduleReconciliation(TParent parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.logger.LogInformation("{operator} operator: scheduling reconciliation for parent {parent}", this.configuration.OperatorName, parent.Metadata?.Name);
            this.reconciliationBuffer.Post(parent.Metadata.Name);
        }

        /// <summary>
        /// Schedules a reconcilation based on a child object, but does not wait for
        /// the reconciliation to be actually performed.
        /// </summary>
        /// <param name="child">
        /// A child object, which has been modified.
        /// </param>
        public void ScheduleReconciliation(TChild child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this.logger.LogInformation("{operator} operator: scheduling reconciliation for child {child}", this.configuration.OperatorName, child.Metadata?.Name);
            this.reconciliationBuffer.Post(child.Metadata.Name);
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
        public async Task ProcessBufferedReconciliationsAsync(CancellationToken cancellationToken)
        {
            if (!await this.processingSemaphore.WaitAsync(0).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Only one instance of ProcessBufferedReconciliationsAsync can run at a time.");
            }

            try
            {
                string name;

                while (!cancellationToken.IsCancellationRequested
                    && (name = await this.reconciliationBuffer.ReceiveAsync(cancellationToken).ConfigureAwait(false)) != null)
                {
                    // Start reading parent and child in parallel
                    var readParentTask = this.parentClient.TryReadAsync(
                        this.configuration.Namespace,
                        name,
                        this.configuration.ParentLabelSelector,
                        cancellationToken);

                    var readChildTask = this.childClient.TryReadAsync(
                        this.configuration.Namespace,
                        name,
                        Selector.Create(this.configuration.ChildLabels),
                        cancellationToken);

                    // Block until we get both
                    var parent = await readParentTask.ConfigureAwait(false);
                    var child = await readChildTask.ConfigureAwait(false);

                    if (parent != null && this.parentFilter(parent))
                    {
                        await this.ReconcileAsync(
                            new ChildOperatorContext<TParent, TChild>(parent, child),
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogWarning("{operator} operator is skipping reconciliation for {name} because the parent was null or filtered out", this.configuration.OperatorName, name);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while executing reconciliations for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
            finally
            {
                this.processingSemaphore.Release();
            }
        }

        /// <summary>
        /// Attempts to reconcile a child and parent object.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ChildOperatorContext{TParent, TChild}"/> object which represents the parent and child object
        /// being reconciled.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task ReconcileAsync(ChildOperatorContext<TParent, TChild> context, CancellationToken cancellationToken)
        {
            // Let's assume Kubernetes takes care of garbage collection, so the source
            // object is always present.
            //
            // Although children with no parents are possible because of cascading background deletions:
            // https://kubernetes.io/docs/concepts/workloads/controllers/garbage-collection/#background-cascading-deletion,
            // they should never enter the reconcile loop.
            Debug.Assert(context.Parent != null, "Cannot have a child object without a parent object");

            if (!await this.reconciliationSemaphore.WaitAsync(0).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Only one instance of ReconcileAsync can run at a time.");
            }

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
                            Labels = new Dictionary<string, string>(this.configuration.ChildLabels),
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
                        "Running {feedbackCount} feedback loops for parent {parent} and child {child} for operator {operatorName} because child is null.",
                        this.feedbackLoops.Count,
                        context.Parent?.Metadata?.Name,
                        context.Child?.Metadata?.Name,
                        this.configuration.OperatorName);

                    JsonPatchDocument<TParent> feedback = null;

                    foreach (var feedbackLoop in this.feedbackLoops)
                    {
                        if ((feedback = await feedbackLoop(context, cancellationToken).ConfigureAwait(false)) != null)
                        {
                            this.logger.LogInformation(
                                "Applying patch {feedback} to parent {parent} for operator {operatorName}.",
                                feedback,
                                context.Parent?.Metadata?.Name,
                                this.configuration.OperatorName);

                            await this.parentClient.PatchAsync(context.Parent, feedback, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Caught error {errorMessage} while executing reconciliation for operator {operator}", ex.Message, this.configuration.OperatorName);
            }
            finally
            {
                this.reconciliationSemaphore.Release();
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
                (eventType, value) =>
                {
                    this.logger.LogInformation("Operator {operator} got an {eventType} event for {value}", this.configuration.OperatorName, eventType, value?.Metadata?.Name);
                    this.ScheduleReconciliation(value);
                    return Task.FromResult(WatchResult.Continue);
                },
                stoppingToken);

            var targetWatch = this.childClient.WatchAsync(
                this.configuration.Namespace,
                fieldSelector: null,
                Selector.Create(this.configuration.ChildLabels),
                null,
                (eventType, value) =>
                {
                    this.logger.LogInformation("Operator {operator} got an {eventType} event for {value}", this.configuration.OperatorName, eventType, value?.Metadata?.Name);
                    this.ScheduleReconciliation(value);
                    return Task.FromResult(WatchResult.Continue);
                },
                stoppingToken);

            // Schedule the initial reconciliations
            await this.InitializeAsync(stoppingToken).ConfigureAwait(false);

            var reconcilerTask = this.ProcessBufferedReconciliationsAsync(stoppingToken);

            await Task.WhenAny(
                sourceWatch,
                targetWatch,
                reconcilerTask).ConfigureAwait(false);
        }
    }
}
