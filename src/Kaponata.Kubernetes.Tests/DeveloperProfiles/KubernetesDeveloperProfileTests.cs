// <copyright file="KubernetesDeveloperProfileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Xunit;

namespace Kaponata.Kubernetes.Tests.DeveloperProfiles
{
    /// <summary>
    /// Tests the <see cref="KubernetesDeveloperProfile"/> class.
    /// </summary>
    public partial class KubernetesDeveloperProfileTests
    {
        /// <summary>
        /// The <see cref="KubernetesDeveloperProfile"/> constructor validates the arguments.
        /// </summary>
        [Fact]
        public void Construtor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new KubernetesDeveloperProfile(null, NullLogger<KubernetesDeveloperProfile>.Instance));
            Assert.Throws<ArgumentNullException>(() => new KubernetesDeveloperProfile(Mock.Of<KubernetesClient>(), null));
        }
    }
}
