// <copyright file="FakeOperatorTests.Ingress.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Operator.Operators;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the ingress-related methods of the <see cref="FakeOperators"/> class.
    /// </summary>
    public partial class FakeOperatorTests
    {
        /// <summary>
        /// Data for the <see cref="BuildIngressOperator_NoFeedback_Async(ChildOperatorContext{WebDriverSession, V1Ingress})"/> theory.
        /// </summary>
        /// <returns>
        /// Test data.
        /// </returns>
        public static IEnumerable<object[]> BuildIngressOperator_NoFeedback_Data()
        {
            // The session has no associated Ingress.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Ingress>(
                   new WebDriverSession()
                   {
                       Status = new WebDriverSessionStatus(),
                   },
                   null),
            };

            // The session has an associated Ingress but the IngressReady flag is already set.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Ingress>(
                   new WebDriverSession()
                   {
                       Status = new WebDriverSessionStatus()
                       {
                           IngressReady = true,
                       },
                   },
                   new V1Ingress()),
            };
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildIngressOperator(IServiceProvider)"/> throws when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void BuildIngressOperator_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>("services", () => FakeOperators.BuildIngressOperator(null));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildIngressOperator(IServiceProvider)"/> correctly populates the standard properties.
        /// </summary>
        [Fact]
        public void BuildIngressOperator_SimpleProperties_Test()
        {
            var builder = FakeOperators.BuildIngressOperator(this.host.Services);

            // Name, namespace and labels
            Assert.Collection(
                builder.Configuration.ChildLabels,
                l =>
                {
                    Assert.Equal(Annotations.ManagedBy, l.Key);
                    Assert.Equal("WebDriverSession-IngressOperator", l.Value);
                });

            Assert.Equal("WebDriverSession-IngressOperator", builder.Configuration.OperatorName);
            Assert.Null(builder.Configuration.ParentLabelSelector);

            // Parent Filter: only sessions with a session id
            Assert.False(builder.ParentFilter(new WebDriverSession()));
            Assert.False(builder.ParentFilter(new WebDriverSession() { Status = new WebDriverSessionStatus() }));
            Assert.True(builder.ParentFilter(new WebDriverSession() { Status = new WebDriverSessionStatus() { SessionId = "1234" } }));

            // Child factory
            Assert.NotNull(builder.ChildFactory);

            // Feedback loop
            Assert.NotEmpty(builder.FeedbackLoops);
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildIngressOperator(IServiceProvider)"/> correctly configures the child pod.
        /// </summary>
        [Fact]
        public void BuildIngressOperator_ConfiguresIngress_Test()
        {
            var builder = FakeOperators.BuildIngressOperator(this.host.Services);

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                },
                Status = new WebDriverSessionStatus()
                {
                    SessionId = "1234",
                },
            };

            var ingress = new V1Ingress();

            builder.ChildFactory(session, ingress);

            var rule = Assert.Single(ingress.Spec.Rules);
            var path = Assert.Single(rule.Http.Paths);
            Assert.Equal("/wd/hub/session/1234/", path.Path);
            Assert.Equal("Prefix", path.PathType);
            Assert.Equal("my-session", path.Backend.Service.Name);
            Assert.Equal(4774, path.Backend.Service.Port.Number);
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildIngressOperator(IServiceProvider)"/> does nto provide any feedback when the sessions or pod are not ready.
        /// </summary>
        /// <param name="context">
        /// The context on which to operate.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(BuildIngressOperator_NoFeedback_Data))]
        public async Task BuildIngressOperator_NoFeedback_Async(ChildOperatorContext<WebDriverSession, V1Ingress> context)
        {
            var builder = FakeOperators.BuildIngressOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);
            Assert.Null(await feedback(context, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildIngressOperator(IServiceProvider)"/> provides correct feedback when session creation fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task BuildIngressOperator_Feedback_Async()
        {
            var builder = FakeOperators.BuildIngressOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);

            var context = new ChildOperatorContext<WebDriverSession, V1Ingress>(
                new WebDriverSession()
                {
                    Status = new WebDriverSessionStatus(),
                },
                new V1Ingress()
                {
                });

            var result = await feedback(context, default).ConfigureAwait(false);
            Assert.Collection(
                result.Operations,
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/ingressReady", o.path);
                    Assert.Equal(true, o.value);
                });
        }
    }
}
