// <copyright file="KubernetesObjectExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.Rest;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="KubernetesObjectExtensions" /> class.
    /// </summary>
    public class KubernetesObjectExtensionsTests
    {
        /// <summary>
        /// The owner reference used by <see cref="KubernetesObjectExtensions.AsOwnerReference(IKubernetesObject{V1ObjectMeta}, bool?, bool?)"/>
        /// returns a reference which matches <see cref="ModelExtensions.IsOwnedBy(IMetadata{V1ObjectMeta}, IKubernetesObject{V1ObjectMeta})"/>.
        /// </summary>
        [Fact]
        public void AsOwner_MatchesOwnedBy()
        {
            var parent = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            var child = new V1Pod();
            child.AddOwnerReference(parent.AsOwnerReference());

            Assert.True(child.IsOwnedBy(parent));
        }

        /// <summary>
        /// <see cref="KubernetesObjectExtensions.AsOwnerReference(IKubernetesObject{V1ObjectMeta}, bool?, bool?)"/>
        /// validates its arguments.
        /// </summary>
        [Fact]
        public void AsOwner_ValidatesArguments()
        {
            Assert.Throws<ValidationException>(() => new V1Pod().AsOwnerReference());
            Assert.Throws<ValidationException>(() => new V1Pod() { Kind = V1Pod.KubeKind }.AsOwnerReference());
            Assert.Throws<ValidationException>(() => new V1Pod() { ApiVersion = V1Pod.KubeApiVersion }.AsOwnerReference());
        }

        /// <summary>
        /// The owner reference used by <see cref="KubernetesObjectExtensions.AsOwnerReference(IKubernetesObject{V1ObjectMeta}, bool?, bool?)"/>
        /// is configured correctly.
        /// </summary>
        [Fact]
        public void AsOwner_ReturnsValidReference()
        {
            var parent = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                    Uid = "my-uid",
                },
            };

            var reference = parent.AsOwnerReference();
            reference.Validate();

            Assert.Equal(V1Pod.KubeApiVersion, reference.ApiVersion);
            Assert.Null(reference.BlockOwnerDeletion);
            Assert.Null(reference.Controller);
            Assert.Equal(V1Pod.KubeKind, reference.Kind);
            Assert.Equal("test", reference.Name);
            Assert.Equal("my-uid", reference.Uid);
        }
    }
}
