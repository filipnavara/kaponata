// <copyright file="ChildOperatorBuilderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Operator.Operators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Kaponata.Operator.Tests.Operators
{
    /// <summary>
    /// Tests the <see cref="ChildOperatorBuilder"/> class.
    /// </summary>
    public class ChildOperatorBuilderTests
    {
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorBuilderTests"/> class.
        /// </summary>
        /// <param name="output">
        /// A <see cref="ITestOutputHelper"/> which captures test output.
        /// </param>
        public ChildOperatorBuilderTests(ITestOutputHelper output)
        {
            var builder = new HostBuilder();
            builder.ConfigureServices(
                (services) =>
                {
                    services.AddKubernetes();
                    services.AddLogging(
                        (loggingBuilder) =>
                        {
                            loggingBuilder.AddXunit(output);
                        });
                    services.AddScoped<ChildOperatorBuilder>();
                });

            this.host = builder.Build();
        }

        /// <summary>
        /// The <see cref="ChildOperatorBuilder"/> constructor validates arguments passed to it.
        /// </summary>
        [Fact]
        public void Builder1_Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("services", () => new ChildOperatorBuilder(null));
        }

        /// <summary>
        /// The <see cref="ChildOperatorBuilder{TParent}"/> constructor validates arguments passed to it.
        /// </summary>
        [Fact]
        public void Builder2_Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("services", () => new ChildOperatorBuilder<WebDriverSession>(null, new ChildOperatorConfiguration(string.Empty)));
            Assert.Throws<ArgumentNullException>("configuration", () => new ChildOperatorBuilder<WebDriverSession>(Mock.Of<IServiceProvider>(), null));
        }

        /// <summary>
        /// The <see cref="ChildOperatorBuilder{TParent, TChild}"/> constructor validates arguments passed to it.
        /// </summary>
        [Fact]
        public void Builder3_Constructor_ValidatesArguments()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var configuration = new ChildOperatorConfiguration(string.Empty);
            Func<WebDriverSession, bool> parentFilter = (session) => true;
            Action<WebDriverSession, V1Pod> childFactory = (session, pod) => { };

            Assert.Throws<ArgumentNullException>("services", () => new ChildOperatorBuilder<WebDriverSession, V1Pod>(null, configuration, parentFilter, childFactory));
            Assert.Throws<ArgumentNullException>("configuration", () => new ChildOperatorBuilder<WebDriverSession, V1Pod>(serviceProvider, null, parentFilter, childFactory));
            Assert.Throws<ArgumentNullException>("parentFilter", () => new ChildOperatorBuilder<WebDriverSession, V1Pod>(serviceProvider, configuration, null, childFactory));
            Assert.Throws<ArgumentNullException>("childFactory", () => new ChildOperatorBuilder<WebDriverSession, V1Pod>(serviceProvider, configuration, parentFilter, null));
        }

        /// <summary>
        /// <see cref="ChildOperatorBuilder{TParent}.Where(Func{TParent, bool})"/> validates the arguments passed to it.
        /// </summary>
        [Fact]
        public void Where_ValidatesArguments()
        {
            var builder = new ChildOperatorBuilder<WebDriverSession>(Mock.Of<IServiceProvider>(), new ChildOperatorConfiguration(string.Empty));
            Assert.Throws<ArgumentNullException>("parentFilter", () => builder.Where(null));
        }

        /// <summary>
        /// <see cref="ChildOperatorBuilder{TParent, TChild}.PostsFeedback"/> validates
        /// arguments passed to it.
        /// </summary>
        [Fact]
        public void PostsFeedback_ValidatesArguments()
        {
            var builder = new ChildOperatorBuilder<WebDriverSession, V1Pod>(Mock.Of<IServiceProvider>(), new ChildOperatorConfiguration(string.Empty), (session) => true, (session, pod) => { });

            Assert.Throws<ArgumentNullException>("feedbackLoop", () => builder.PostsFeedback(null));
        }

        /// <summary>
        /// Simulates the building of a complex operator.
        /// </summary>
        [Fact]
        public void CanBuildComplexOperator()
        {
            var builder = this.host.Services.GetRequiredService<ChildOperatorBuilder>();

            var @operator = builder
                .CreateOperator("my-operator")
                .Watches<WebDriverSession>()
                    .WithLabels((session) => session.Metadata.Annotations[Annotations.AutomationName] == Annotations.AutomationNames.Fake)
                    .Where((session) => session.Status?.SessionId == null)
                .Creates<V1Pod>((session, pod) =>
                {
                    pod.Spec = new V1PodSpec()
                    {
                        Containers = new V1Container[]
                        {
                            new V1Container()
                            {
                                Image = "quay.io/kaponata/fake-driver:2.0.1",
                            },
                        },
                    };
                })
                .PostsFeedback((context, cancellationToken) =>
                {
                    var session = context.Parent;
                    var pod = context.Child;

                    JsonPatchDocument<WebDriverSession> patch;

                    if (session.Status?.SessionId != null)
                    {
                        patch = null;
                    }
                    else if (pod.Status.Phase != "Running" || !pod.Status.ContainerStatuses.All(c => c.Ready))
                    {
                        patch = null;
                    }
                    else
                    {
                        patch = new JsonPatchDocument<WebDriverSession>();
                        patch.Add(s => s.Status, new WebDriverSessionStatus() { SessionId = Guid.NewGuid().ToString() });
                    }

                    return Task.FromResult(patch);
                }).Build();
        }
    }
}
