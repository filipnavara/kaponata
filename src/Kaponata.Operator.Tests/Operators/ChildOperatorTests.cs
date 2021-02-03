// <copyright file="ChildOperatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Kaponata.Operator.Models;
using Kaponata.Operator.Operators;
using MELT;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tesets the <see cref="ChildOperator{TParent, TChild}"/> class.
    /// </summary>
    public class ChildOperatorTests
    {
        private readonly KubernetesClient kubernetes = Mock.Of<KubernetesClient>();
        private readonly ChildOperatorConfiguration configuration;
        private readonly Func<WebDriverSession, bool> filter = (session) => true;
        private readonly Action<WebDriverSession, V1Pod> factory = (session, pod) => { };
        private readonly ILogger<ChildOperator<WebDriverSession, V1Pod>> logger = NullLogger<ChildOperator<WebDriverSession, V1Pod>>.Instance;
        private readonly Collection<ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop> feedbackLoops = new Collection<ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorTests"/> class.
        /// </summary>
        public ChildOperatorTests()
        {
            this.configuration = new ChildOperatorConfiguration(nameof(ChildOperatorTests))
            {
                ParentLabelSelector = "parent-label-selector",
            };
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}"/> constructor throws an exception
        /// when passed <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void Constructor_ArgumentNull_Throws()
        {
            Assert.Throws<ArgumentNullException>("kubernetes", () => new ChildOperator<WebDriverSession, V1Pod>(null, this.configuration, this.filter, this.factory, this.feedbackLoops, this.logger));
            Assert.Throws<ArgumentNullException>("configuration", () => new ChildOperator<WebDriverSession, V1Pod>(this.kubernetes, null, this.filter, this.factory, this.feedbackLoops, this.logger));
            Assert.Throws<ArgumentNullException>("parentFilter", () => new ChildOperator<WebDriverSession, V1Pod>(this.kubernetes, this.configuration, null, this.factory, this.feedbackLoops, this.logger));
            Assert.Throws<ArgumentNullException>("childFactory", () => new ChildOperator<WebDriverSession, V1Pod>(this.kubernetes, this.configuration, this.filter, null, this.feedbackLoops, this.logger));
            Assert.Throws<ArgumentNullException>("feedbackLoops", () => new ChildOperator<WebDriverSession, V1Pod>(this.kubernetes, this.configuration, this.filter, this.factory, null, this.logger));
            Assert.Throws<ArgumentNullException>("logger", () => new ChildOperator<WebDriverSession, V1Pod>(this.kubernetes, this.configuration, this.filter, this.factory, this.feedbackLoops, null));
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}.ReconcileAsync"/> method does nothing if the child object
        /// exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReconcileAsync_Child_NoOp_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessions = kubernetes.WithClient<WebDriverSession>();
            var pods = kubernetes.WithClient<V1Pod>();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) =>
                {
                    pod.Spec = new V1PodSpec(
                        new V1Container[]
                        {
                            new V1Container()
                            {
                                Name = "fake-driver",
                                Image = "quay.io/kaponata/fake-driver:2.0.1",
                            },
                        });
                },
                this.feedbackLoops,
                this.logger))
            {
                var parent = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                        Uid = "my-uid",
                    },
                };

                var child = new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                    },
                };

                await @operator.ReconcileAsync(
                    new (parent, child),
                    default).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}.ReconcileAsync"/> method logs exceptions which
        /// are encountered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReconcileAsync_Exception_LogsError_Async()
        {
            var loggerFactory = TestLoggerFactory.Create();
            var logger = loggerFactory.CreateLogger<ChildOperator<WebDriverSession, V1Pod>>();

            var kubernetes = new Mock<KubernetesClient>();
            var sessions = kubernetes.WithClient<WebDriverSession>();
            var pods = kubernetes.WithClient<V1Pod>();
            pods.Setup(p => p.CreateAsync(It.IsAny<V1Pod>(), default)).ThrowsAsync(new NotSupportedException());

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                logger))
            {
                var parent = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta(),
                };

                await @operator.ReconcileAsync(
                    new (parent, null),
                    default).ConfigureAwait(false);
            }

            Assert.Collection(
                loggerFactory.Sink.LogEntries,
                e => Assert.Equal("Scheduling reconciliation for parent (null) and child (null) for operator ChildOperatorTests", e.Message),
                e => Assert.Equal("Caught error Specified method is not supported. while executing reconciliation for operator ChildOperatorTests", e.Message));
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}.ReconcileAsync"/> method creates the child object if it
        /// does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReconcileAsync_NoChild_CreatesChild_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessions = kubernetes.WithClient<WebDriverSession>();
            var pods = kubernetes.WithClient<V1Pod>();
            (var createdPods, var _) = pods.TrackCreatedItems();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) =>
                {
                    pod.Spec = new V1PodSpec(
                        new V1Container[]
                        {
                            new V1Container()
                            {
                                Name = "fake-driver",
                                Image = "quay.io/kaponata/fake-driver:2.0.1",
                            },
                        });
                },
                this.feedbackLoops,
                this.logger))
            {
                var parent = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                        Uid = "my-uid",
                    },
                };

                await @operator.ReconcileAsync(
                    new (parent, null),
                    default).ConfigureAwait(false);
            }

            Assert.Collection(
                createdPods,
                p =>
                {
                    // The name, namespace and owner references are initialized correctly
                    Assert.Equal("my-session", p.Metadata.Name);
                    Assert.Equal("default", p.Metadata.NamespaceProperty);

                    Assert.Collection(
                        p.Metadata.OwnerReferences,
                        r =>
                        {
                            Assert.Equal("my-session", r.Name);
                            Assert.Equal("WebDriverSession", r.Kind);
                            Assert.Equal("kaponata.io/v1alpha1", r.ApiVersion);
                            Assert.Equal("my-uid", r.Uid);
                        });

                    // The labels are copied from the configuration
                    Assert.Collection(
                        p.Metadata.Labels,
                        p =>
                        {
                            Assert.Equal(Annotations.ManagedBy, p.Key);
                            Assert.Equal(nameof(ChildOperatorTests), p.Value);
                        });
                });
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}.ReconcileAsync"/> method executes the feedback
        /// loop when parent and child are present, and performs the patch returned by the feedback loop.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReconcileAsync_FeedbackLoopHasPatch_ExecutesPatch_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessions = kubernetes.WithClient<WebDriverSession>();
            var pods = kubernetes.WithClient<V1Pod>();

            var patch = new JsonPatchDocument<WebDriverSession>();
            patch.Add(s => s.Status, new WebDriverSessionStatus());

            var parent = new WebDriverSession();
            var child = new V1Pod();

            sessions.Setup(s => s.PatchAsync(parent, patch, default))
                .ReturnsAsync(parent)
                .Verifiable();

            var feedbackLoops = new Collection<ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop>()
            {
                new ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop((context, cancellationToken) =>
                {
                    return Task.FromResult(patch);
                }),
            };

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                feedbackLoops,
                this.logger))
            {
                await @operator.ReconcileAsync(
                    new (parent, child),
                    default).ConfigureAwait(false);
            }

            sessions.Verify();
        }

        /// <summary>
        /// The <see cref="ChildOperator{TParent, TChild}.ReconcileAsync"/> method executes the feedback
        /// loop when parent and child are present, and does nothing when the feedback loop returns <see langword="null"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReconcileAsync_FeedbackLoopReturnsNull_DoesNothing_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessions = kubernetes.WithClient<WebDriverSession>();
            var pods = kubernetes.WithClient<V1Pod>();

            JsonPatchDocument<WebDriverSession> patch = null;

            var parent = new WebDriverSession();
            var child = new V1Pod();

            var feedbackLoops = new Collection<ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop>()
            {
                new ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop((context, cancellationToken) =>
                {
                    return Task.FromResult(patch);
                }),
            };

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                feedbackLoops,
                this.logger))
            {
                await @operator.ReconcileAsync(
                    new (parent, child),
                    default).ConfigureAwait(false);
            }

            sessions.Verify();
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.InitializeAsync"/> schedules reconciliation
        /// for a parent which does not have a child.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InitializeAsync_ParentWithoutChild_SchedulesReconciliation_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            sessionClient.WithList(
                null,
                "parent-label-selector",
                parent);

            podClient.WithList(
                null,
                labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests");

            var createdPods = podClient.TrackCreatedItems();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);

                Assert.True(@operator.ReconcilationBuffer.TryReceive(null, out var context));

                Assert.Equal(parent, context.Parent);
                Assert.Null(context.Child);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.InitializeAsync"/> skips parents which are
        /// filtered out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InitializeAsync_FilteredParent_IsSkipped_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            sessionClient.WithList(
                null,
                "parent-label-selector",
                parent);

            podClient.WithList(
                null,
                labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests");

            var createdPods = podClient.TrackCreatedItems();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                (session) => false, /* filter out _all_ parents */
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.InitializeAsync"/> schedules reconciliation
        /// for a parent which does have a child.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task InitializeAsync_ParentWithChild_SchedulesReconciliation_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                    OwnerReferences = new V1OwnerReference[]
                    {
                        new V1OwnerReference()
                        {
                            ApiVersion = WebDriverSession.KubeApiVersion,
                            Kind = WebDriverSession.KubeKind,
                            Name = parent.Metadata.Name,
                            Uid = parent.Metadata.Uid,
                        },
                    },
                },
            };

            sessionClient.WithList(
                null,
                "parent-label-selector",
                parent);

            podClient.WithList(
                null,
                labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests",
                child);

            var createdPods = podClient.TrackCreatedItems();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);

                Assert.True(@operator.ReconcilationBuffer.TryReceive(null, out var context));

                Assert.Equal(parent, context.Parent);
                Assert.Equal(child, context.Child);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.InitializeAsync(CancellationToken)"/> catches
        /// and handles exceptions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task InitializeAsync_HandlesException_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            sessionClient
                .Setup(s => s.ListAsync("default", null, null, "parent-label-selector", null, default))
                .ThrowsAsync(new NotSupportedException());

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.InitializeAsync(default).ConfigureAwait(false);

                await Assert.ThrowsAsync<NotSupportedException>(() => @operator.InitializationCompleted).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TParent, CancellationToken)"/>
        /// validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleParentReconciliationAsync_ValidatesArgument_Async()
        {
            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                this.kubernetes,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("parent", () => @operator.ScheduleReconciliationAsync((WebDriverSession)null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TParent, CancellationToken)"/> catches and logs errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleParentReconciliationAsync_HandlesException_Async()
        {
            var loggerFactory = TestLoggerFactory.Create();
            var logger = loggerFactory.CreateLogger<ChildOperator<WebDriverSession, V1Pod>>();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();
            podClient
                .Setup(p => p.ListAsync("default", null, "metadata.name=my-session", "app.kubernetes.io/managed-by=ChildOperatorTests", null, default))
                .ThrowsAsync(new NotSupportedException());

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                logger))
            {
                await @operator.ScheduleReconciliationAsync(new WebDriverSession() { Metadata = new V1ObjectMeta() { Name = "my-session" } }, default).ConfigureAwait(false);

                Assert.Collection(
                    loggerFactory.Sink.LogEntries,
                    e => Assert.Equal("ChildOperatorTests operator: scheduling reconciliation for parent my-session", e.Message),
                    e => Assert.Equal("Caught error Specified method is not supported. while scheduling parent reconciliation for operator ChildOperatorTests", e.Message));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TParent, CancellationToken)"/>
        /// posts a new item to to the queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleParentReconciliationAsync_PostsToQueue_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            podClient.WithList(
                fieldSelector: "metadata.name=my-session",
                labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests",
                child);

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.ScheduleReconciliationAsync(parent, default).ConfigureAwait(false);

                Assert.True(@operator.ReconcilationBuffer.TryReceive(null, out var context));

                Assert.Equal(parent, context.Parent);
                Assert.Equal(child, context.Child);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TParent, CancellationToken)"/>
        /// skips parents which are filtered out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleParentReconciliationAsync_ParentFiltered_Skipped_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            podClient.WithList(
                fieldSelector: "metadata.name=my-session",
                labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests",
                child);

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                (session) => false, /* all parents are filtered out */
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.ScheduleReconciliationAsync(parent, default).ConfigureAwait(false);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TChild, CancellationToken)"/>
        /// validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleChildReconciliationAsync_ValidatesArgument_Async()
        {
            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                this.kubernetes,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await Assert.ThrowsAsync<ArgumentNullException>("child", () => @operator.ScheduleReconciliationAsync((V1Pod)null, default)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TChild, CancellationToken)"/> catches and logs errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleChildReconciliationAsync_HandlesException_Async()
        {
            var loggerFactory = TestLoggerFactory.Create();
            var logger = loggerFactory.CreateLogger<ChildOperator<WebDriverSession, V1Pod>>();

            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            sessionClient
                .Setup(p => p.ListAsync("default", null, "metadata.name=my-session", "parent-label-selector", null, default))
                .ThrowsAsync(new NotSupportedException());

            var podClient = kubernetes.WithClient<V1Pod>();

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                logger))
            {
                await @operator.ScheduleReconciliationAsync(new V1Pod() { Metadata = new V1ObjectMeta() { Name = "my-session" } }, default).ConfigureAwait(false);

                Assert.Collection(
                    loggerFactory.Sink.LogEntries,
                    e => Assert.Equal("ChildOperatorTests operator: scheduling reconciliation for child my-session", e.Message),
                    e => Assert.Equal("Caught error Specified method is not supported. while scheduling child reconciliation for operator ChildOperatorTests", e.Message));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TChild, CancellationToken)"/>
        /// posts a new item to to the queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleChildReconciliationAsync_PostsToQueue_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            sessionClient.WithList(
                fieldSelector: "metadata.name=my-session",
                labelSelector: "parent-label-selector",
                parent);

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.ScheduleReconciliationAsync(child, default).ConfigureAwait(false);

                Assert.True(@operator.ReconcilationBuffer.TryReceive(null, out var context));

                Assert.Equal(parent, context.Parent);
                Assert.Equal(child, context.Child);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TChild, CancellationToken)"/>
        /// posts a new item to to the queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleChildReconciliationAsync_NoParent_Skipped_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();

            sessionClient.WithList(
                fieldSelector: "metadata.name=my-session",
                labelSelector: "parent-label-selector");

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.ScheduleReconciliationAsync(child, default).ConfigureAwait(false);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ScheduleReconciliationAsync(TChild, CancellationToken)"/>
        /// skips children if their parent is skipped.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ScheduleChildReconciliationAsync_FilteredParent_Skipped_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();

            var parent = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            sessionClient.WithList(
                fieldSelector: "metadata.name=my-session",
                labelSelector: "parent-label-selector",
                parent);

            var child = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                (session) => false, /* all parents are filtered out */
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                await @operator.ScheduleReconciliationAsync(child, default).ConfigureAwait(false);

                Assert.False(@operator.ReconcilationBuffer.TryReceive(null, out var _));
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ExecuteAsync(CancellationToken)"/> can stop and start correctly
        /// when the watchers generate no events.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ExecuteAsync_StopStart_Succeeds_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var sessionWatcher = sessionClient.WithWatcher(
                null,
                "parent-label-selector");

            var podWatcher = podClient.WithWatcher(
                null,
                "app.kubernetes.io/managed-by=ChildOperatorTests");

            sessionClient.WithList(null, "parent-label-selector");
            podClient.WithList(null, "app.kubernetes.io/managed-by=ChildOperatorTests");

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                new Collection<ChildOperator<WebDriverSession, V1Pod>.FeedbackLoop>(),
                this.logger))
            {
                // Wait for the operator to start and complete initialization
                await Task.WhenAll(
                    @operator.StartAsync(default),
                    @operator.InitializationCompleted,
                    sessionWatcher.ClientRegistered.Task,
                    podWatcher.ClientRegistered.Task).ConfigureAwait(false);

                await @operator.StopAsync(default).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ExecuteAsync(CancellationToken)"/> reacts to parent
        /// events and schedules reconciliation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ExecuteAsync_ParentEvent_IsProcessed_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var sessionWatcher = sessionClient.WithWatcher(
                null,
                "parent-label-selector");

            var podWatcher = podClient.WithWatcher(
                null,
                "app.kubernetes.io/managed-by=ChildOperatorTests");

            sessionClient.WithList(null, "parent-label-selector");
            podClient.WithList(null, "app.kubernetes.io/managed-by=ChildOperatorTests");

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                // Wait for the operator to start and complete initialization
                await @operator.StartAsync(default).ConfigureAwait(false);
                await @operator.InitializationCompleted.ConfigureAwait(false);

                // Similate a parent event, this should result in a child object being created.
                var sessionWatchClient = await sessionWatcher.ClientRegistered.Task.ConfigureAwait(false);

                var parent = new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                    },
                };

                // Let's assume there's no child for this parent:
                podClient.WithList(
                    fieldSelector: "metadata.name=my-session",
                    labelSelector: "app.kubernetes.io/managed-by=ChildOperatorTests");

                // And capture the creation of this new child:
                (var _, var firstPodCreated) = podClient.TrackCreatedItems();

                Assert.Equal(WatchResult.Continue, await sessionWatchClient(k8s.WatchEventType.Added, parent).ConfigureAwait(false));
                var pod = await firstPodCreated.ConfigureAwait(false);

                Assert.Equal("my-session", pod.Metadata.Name);

                await @operator.StopAsync(default).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <see cref="ChildOperator{TParent, TChild}.ExecuteAsync(CancellationToken)"/> reacts to child
        /// events and schedules reconciliation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ExecuteAsync_ChildEvent_IsProcessed_Async()
        {
            var kubernetes = new Mock<KubernetesClient>();
            var sessionClient = kubernetes.WithClient<WebDriverSession>();
            var podClient = kubernetes.WithClient<V1Pod>();

            var sessionWatcher = sessionClient.WithWatcher(
                null,
                "parent-label-selector");

            var podWatcher = podClient.WithWatcher(
                null,
                "app.kubernetes.io/managed-by=ChildOperatorTests");

            sessionClient.WithList(null, "parent-label-selector");
            podClient.WithList(null, "app.kubernetes.io/managed-by=ChildOperatorTests");

            using (var @operator = new ChildOperator<WebDriverSession, V1Pod>(
                kubernetes.Object,
                this.configuration,
                this.filter,
                (session, pod) => { },
                this.feedbackLoops,
                this.logger))
            {
                // Wait for the operator to start and complete initialization
                await @operator.StartAsync(default).ConfigureAwait(false);
                await @operator.InitializationCompleted.ConfigureAwait(false);

                // Similate a parent event, this should result in a child object being created.
                var podWatchClient = await podWatcher.ClientRegistered.Task.ConfigureAwait(false);

                var child = new V1Pod()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Name = "my-session",
                        NamespaceProperty = "default",
                    },
                };

                // Let's assume there's a parent for this child.
                sessionClient.WithList(
                    fieldSelector: "metadata.name=my-session",
                    labelSelector: "parent-label-selector",
                    new WebDriverSession());

                Assert.Equal(WatchResult.Continue, await podWatchClient(k8s.WatchEventType.Added, child).ConfigureAwait(false));

                await @operator.StopAsync(default).ConfigureAwait(false);
            }
        }
    }
}
