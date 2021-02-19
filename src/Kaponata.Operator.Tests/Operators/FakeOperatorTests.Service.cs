// <copyright file="FakeOperatorTests.Service.cs" company="Quamotion bv">
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
    /// Tests service-related functionality in the <see cref="FakeOperators"/> class.
    /// </summary>
    public partial class FakeOperatorTests
    {
        /// <summary>
        /// Data for the <see cref="BuildServiceOperator_NoFeedback_Async(ChildOperatorContext{WebDriverSession, V1Service})"/> theory.
        /// </summary>
        /// <returns>
        /// Test data.
        /// </returns>
        public static IEnumerable<object[]> BuildServiceOperator_NoFeedback_Data()
        {
            // The session has no associated service.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Service>(
                   new WebDriverSession()
                   {
                       Status = new WebDriverSessionStatus(),
                   },
                   null),
            };

            // The session has an associated service but the serviceReady flag is already set.
            yield return new object[]
            {
               new ChildOperatorContext<WebDriverSession, V1Service>(
                   new WebDriverSession()
                   {
                       Status = new WebDriverSessionStatus()
                       {
                           ServiceReady = true,
                       },
                   },
                   new V1Service()),
            };
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildServiceOperator(IServiceProvider)"/> throws when passed a <see langword="null"/> value.
        /// </summary>
        [Fact]
        public void BuildServiceOperator_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>("services", () => FakeOperators.BuildServiceOperator(null));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildServiceOperator(IServiceProvider)"/> correctly populates the standard properties.
        /// </summary>
        [Fact]
        public void BuildServiceOperator_SimpleProperties_Test()
        {
            var builder = FakeOperators.BuildServiceOperator(this.host.Services);

            // Name, namespace and labels
            Assert.Collection(
                builder.Configuration.ChildLabels,
                l =>
                {
                    Assert.Equal(Annotations.ManagedBy, l.Key);
                    Assert.Equal("WebDriverSession-ServiceOperator", l.Value);
                });

            Assert.Equal("WebDriverSession-ServiceOperator", builder.Configuration.OperatorName);
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
        /// <see cref="FakeOperators.BuildServiceOperator(IServiceProvider)"/> correctly configures the child pod.
        /// </summary>
        /// <param name="sessionPort">
        /// The port at which the Appium server is listening.
        /// </param>
        [Theory]
        [InlineData(4774)]
        [InlineData(4723)]
        public void BuildServiceOperator_ConfiguresService_Test(int sessionPort)
        {
            var builder = FakeOperators.BuildServiceOperator(this.host.Services);

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "my-session",
                },
                Status = new WebDriverSessionStatus()
                {
                    SessionPort = sessionPort,
                },
            };

            var service = new V1Service();

            builder.ChildFactory(session, service);

            Assert.Collection(
                service.Spec.Selector,
                l =>
                {
                    Assert.Equal(Annotations.SessionName, l.Key);
                    Assert.Equal("my-session", l.Value);
                });

            var port = Assert.Single(service.Spec.Ports);
            Assert.Equal(sessionPort, port.TargetPort);
            Assert.Equal(sessionPort, port.Port);
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildServiceOperator(IServiceProvider)"/> does nto provide any feedback when the sessions or pod are not ready.
        /// </summary>
        /// <param name="context">
        /// The context on which to operate.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(BuildServiceOperator_NoFeedback_Data))]
        public async Task BuildServiceOperator_NoFeedback_Async(ChildOperatorContext<WebDriverSession, V1Service> context)
        {
            var builder = FakeOperators.BuildServiceOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);
            Assert.Null(await feedback(context, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="FakeOperators.BuildServiceOperator(IServiceProvider)"/> provides correct feedback when session creation fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task BuildServiceOperator_Feedback_Async()
        {
            var builder = FakeOperators.BuildServiceOperator(this.host.Services);
            var feedback = Assert.Single(builder.FeedbackLoops);

            var context = new ChildOperatorContext<WebDriverSession, V1Service>(
                new WebDriverSession()
                {
                    Status = new WebDriverSessionStatus(),
                },
                new V1Service()
                {
                });

            var result = await feedback(context, default).ConfigureAwait(false);
            Assert.Collection(
                result.Operations,
                o =>
                {
                    Assert.Equal(OperationType.Add, o.OperationType);
                    Assert.Equal("/status/serviceReady", o.path);
                    Assert.Equal(true, o.value);
                });
        }
    }
}
